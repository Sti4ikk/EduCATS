using System.ComponentModel;
using System.Runtime.CompilerServices;
using EduCATS.Controls.RoundedListView.Enums;
using EduCATS.Controls.RoundedListView.Interfaces;
using Newtonsoft.Json;
using Nyxbull.Plugins.CrossLocalization;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Common part of personal and group chat list items:
	/// unread counter and local pin/mute settings.
	/// </summary>
	public abstract class ChatListItemModel : IRoundedListType, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[JsonProperty("id")]
		public int Id { get; set; }

		/// <summary>
		/// Is it a group chat (personal and group chat ids may collide).
		/// </summary>
		[JsonIgnore]
		public abstract bool IsGroupChat { get; }

		int _unread;

		[JsonProperty("unread")]
		public int Unread
		{
			get => _unread;
			set => SetProperty(ref _unread, value);
		}

		bool _isPinned;

		[JsonIgnore]
		public bool IsPinned
		{
			get => _isPinned;
			set
			{
				if (SetProperty(ref _isPinned, value))
				{
					OnPropertyChanged(nameof(PinActionText));
				}
			}
		}

		bool _isMuted;

		[JsonIgnore]
		public bool IsMuted
		{
			get => _isMuted;
			set
			{
				if (SetProperty(ref _isMuted, value))
				{
					OnPropertyChanged(nameof(MuteActionText));
				}
			}
		}

		[JsonIgnore]
		public string PinActionText => CrossLocalization.Translate(IsPinned ? "chat_unpin" : "chat_pin");

		[JsonIgnore]
		public string MuteActionText => CrossLocalization.Translate(IsMuted ? "chat_unmute" : "chat_mute");

		public RoundedListTypeEnum GetListType() => RoundedListTypeEnum.Navigation;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
		{
			if (Equals(storage, value))
			{
				return false;
			}

			storage = value;
			OnPropertyChanged(propertyName);
			return true;
		}
	}
}
