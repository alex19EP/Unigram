using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Td.Api;
using Template10.Common;
using Template10.Services.NavigationService;
using Unigram.Common;
using Unigram.Controls;
using Unigram.Controls.Views;
using Unigram.Services;
using Unigram.Entities;
using Unigram.Views;
using Unigram.Views.Settings;
using Unigram.Views.SignIn;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Unigram.ViewModels.SignIn
{
    public class SignInViewModel : TLViewModelBase
    {
        private readonly ILifetimeService _lifetimeService;
        private readonly INotificationsService _notificationsService;

        public SignInViewModel(IProtoService protoService, ICacheService cacheService, ISettingsService settingsService, IEventAggregator aggregator, ILifetimeService lifecycleService, INotificationsService notificationsService)
            : base(protoService, cacheService, settingsService, aggregator)
        {
            _lifetimeService = lifecycleService;
            _notificationsService = notificationsService;

            SendCommand = new RelayCommand(SendExecute, () => !IsLoading);
            ProxyCommand = new RelayCommand(ProxyExecute);
        }

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            ProtoService.Send(new GetCountryCode(), result =>
            {
                if (result is Text text)
                {
                    BeginOnUIThread(() => GotUserCountry(text.TextValue));
                }
            });

            IsLoading = false;
            return Task.CompletedTask;
        }

        private void GotUserCountry(string code)
        {
            Country country = null;
            foreach (var local in Country.Countries)
            {
                if (string.Equals(local.Code, code, StringComparison.OrdinalIgnoreCase))
                {
                    country = local;
                    break;
                }
            }

            if (country != null && SelectedCountry == null && string.IsNullOrEmpty(PhoneNumber))
            {
                BeginOnUIThread(() =>
                {
                    SelectedCountry = country;
                });
            }
        }

        private Country _selectedCountry;
        public Country SelectedCountry
        {
            get
            {
                return _selectedCountry;
            }
            set
            {
                if (value != null)
                {
                    _phoneCode = value.PhoneCode;
                    RaisePropertyChanged(() => PhoneCode);
                }

                Set(ref _selectedCountry, value);
            }
        }

        private string _phoneCode;
        public string PhoneCode
        {
            get
            {
                return _phoneCode;
            }
            set
            {
                Set(ref _phoneCode, value);

                Country country = null;
                foreach (var c in Country.Countries)
                {
                    if (c.PhoneCode == PhoneCode)
                    {
                        if (c.PhoneCode == "1" && c.Code != "us")
                        {
                            continue;
                        }

                        if (c.PhoneCode == "7" && c.Code != "ru")
                        {
                            continue;
                        }

                        country = c;
                        break;
                    }
                }

                SelectedCountry = country;
            }
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get
            {
                return _phoneNumber;
            }
            set
            {
                Set(ref _phoneNumber, value);
            }
        }

        public IList<Country> Countries { get; } = Country.Countries.OrderBy(x => x.DisplayName).ToList();

        public RelayCommand SendCommand { get; }
        private async void SendExecute()
        {
            if (string.IsNullOrEmpty(_phoneCode))
            {
                RaisePropertyChanged("PHONE_CODE_INVALID");
                return;
            }

            if (string.IsNullOrEmpty(_phoneNumber))
            {
                RaisePropertyChanged("PHONE_NUMBER_INVALID");
                return;
            }

            var phoneNumber = (_phoneCode + _phoneNumber).Replace(" ", string.Empty);

            foreach (var session in _lifetimeService.Items)
            {
                var user = session.ProtoService.GetUser(session.UserId);
                if (user == null)
                {
                    continue;
                }

                if (user.PhoneNumber.Contains(phoneNumber) || phoneNumber.Contains(user.PhoneNumber))
                {
                    var confirm = await TLMessageDialog.ShowAsync(Strings.Resources.AccountAlreadyLoggedIn, Strings.Resources.AppName, Strings.Resources.AccountSwitch, Strings.Resources.OK);
                    if (confirm == ContentDialogResult.Primary)
                    {
                        _lifetimeService.PreviousItem = session;
                        ProtoService.Send(new Destroy());
                    }

                    return;
                }
            }

            IsLoading = true;

            await _notificationsService.CloseAsync();

            var response = await ProtoService.SendAsync(new SetAuthenticationPhoneNumber(phoneNumber, new PhoneNumberAuthenticationSettings(false, false, string.Empty, string.Empty)));
            if (response is Error error)
            {
                IsLoading = false;

                if (error.TypeEquals(ErrorType.PHONE_NUMBER_INVALID))
                {
                    //needShowInvalidAlert(req.phone_number, false);
                    await TLMessageDialog.ShowAsync(Strings.Resources.InvalidPhoneNumber, Strings.Resources.AppName, Strings.Resources.OK);
                }
                else if (error.TypeEquals(ErrorType.PHONE_PASSWORD_FLOOD))
                {
                    await TLMessageDialog.ShowAsync(Strings.Resources.FloodWait, Strings.Resources.AppName, Strings.Resources.OK);
                }
                else if (error.TypeEquals(ErrorType.PHONE_NUMBER_FLOOD))
                {
                    await TLMessageDialog.ShowAsync(Strings.Resources.PhoneNumberFlood, Strings.Resources.AppName, Strings.Resources.OK);
                }
                else if (error.TypeEquals(ErrorType.PHONE_NUMBER_BANNED))
                {
                    //needShowInvalidAlert(req.phone_number, true);
                    await TLMessageDialog.ShowAsync(Strings.Resources.BannedPhoneNumber, Strings.Resources.AppName, Strings.Resources.OK);
                }
                else if (error.TypeEquals(ErrorType.PHONE_CODE_EMPTY) || error.TypeEquals(ErrorType.PHONE_CODE_INVALID))
                {
                    await TLMessageDialog.ShowAsync(Strings.Resources.InvalidCode, Strings.Resources.AppName, Strings.Resources.OK);
                }
                else if (error.TypeEquals(ErrorType.PHONE_CODE_EXPIRED))
                {
                    await TLMessageDialog.ShowAsync(Strings.Resources.CodeExpired, Strings.Resources.AppName, Strings.Resources.OK);
                }
                else if (error.Message.StartsWith("FLOOD_WAIT"))
                {
                    await TLMessageDialog.ShowAsync(Strings.Resources.FloodWait, Strings.Resources.AppName, Strings.Resources.OK);
                }
                else if (error.Code != -1000)
                {
                    await TLMessageDialog.ShowAsync(error.Message, Strings.Resources.AppName, Strings.Resources.OK);
                }
            }
        }

        public RelayCommand ProxyCommand { get; }
        private void ProxyExecute()
        {
            NavigationService.Navigate(typeof(SettingsProxiesPage));
        }
    }
}