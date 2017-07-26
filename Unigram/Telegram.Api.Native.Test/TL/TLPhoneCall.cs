// <auto-generated/>
using System;
using Telegram.Api.Native.TL;

namespace Telegram.Api.TL
{
	public partial class TLPhoneCall : TLPhoneCallBase 
	{
		public Int64 AccessHash { get; set; }
		public Int32 Date { get; set; }
		public Int32 AdminId { get; set; }
		public Int32 ParticipantId { get; set; }
		public Byte[] GAOrB { get; set; }
		public Int64 KeyFingerprint { get; set; }
		public TLPhoneCallProtocol Protocol { get; set; }
		public TLPhoneConnection Connection { get; set; }
		public TLVector<TLPhoneConnection> AlternativeConnections { get; set; }
		public Int32 StartDate { get; set; }

		public TLPhoneCall() { }
		public TLPhoneCall(TLBinaryReader from)
		{
			Read(from);
		}

		public override TLType TypeId { get { return TLType.PhoneCall; } }

		public override void Read(TLBinaryReader from)
		{
			Id = from.ReadInt64();
			AccessHash = from.ReadInt64();
			Date = from.ReadInt32();
			AdminId = from.ReadInt32();
			ParticipantId = from.ReadInt32();
			GAOrB = from.ReadByteArray();
			KeyFingerprint = from.ReadInt64();
			Protocol = TLFactory.Read<TLPhoneCallProtocol>(from);
			Connection = TLFactory.Read<TLPhoneConnection>(from);
			AlternativeConnections = TLFactory.Read<TLVector<TLPhoneConnection>>(from);
			StartDate = from.ReadInt32();
		}

		public override void Write(TLBinaryWriter to)
		{
			to.WriteInt64(Id);
			to.WriteInt64(AccessHash);
			to.WriteInt32(Date);
			to.WriteInt32(AdminId);
			to.WriteInt32(ParticipantId);
			to.WriteByteArray(GAOrB);
			to.WriteInt64(KeyFingerprint);
			to.WriteObject(Protocol);
			to.WriteObject(Connection);
			to.WriteObject(AlternativeConnections);
			to.WriteInt32(StartDate);
		}
	}
}