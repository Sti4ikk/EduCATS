using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EduCATS.Pages.Chat.Models
{
	/// <summary>
	/// Mirrors server-side Entities.DTO.MessageDto.
	/// </summary>
	/// <remarks>
	/// Also used for messages that are being sent from this device
	/// (<see cref="LocalId"/> is set for them) - the server echoes
	/// the message back and it replaces the local one.
	/// </remarks>
	public class MessageItemModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("text")]
		public string Text { get; set; }

		[JsonProperty("time")]
		public System.DateTime Time { get; set; }

		[JsonProperty("imageContent")]
		public string[] ImageContent { get; set; }

		[JsonProperty("isimage")]
		public bool? IsImage { get; set; }

		[JsonProperty("isfile")]
		public bool? IsFile { get; set; }

		[JsonProperty("fileContent")]
		public string FileContent { get; set; }

		[JsonProperty("fileSize")]
		public string FileSize { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("profile")]
		public string Profile { get; set; }

		[JsonProperty("align")]
		public string Align { get; set; }

		[JsonProperty("chatId")]
		public int ChatId { get; set; }

		[JsonIgnore]
		public bool IsMine { get; set; }

		/// <summary>
		/// Is the message written by the current user.
		/// </summary>
		/// <remarks>
		/// The server sets <see cref="Align"/> to "right" for the recipient's own
		/// messages (the web client relies on it too). The DTO has no author id,
		/// so the name is compared only if the server sent no alignment -
		/// namesakes would be mixed up.
		/// </remarks>
		/// <param name="currentUserName">Current user's full name.</param>
		/// <returns><c>true</c> for own messages.</returns>
		public bool IsWrittenBy(string currentUserName) =>
			!string.IsNullOrEmpty(Align) ?
				string.Equals(Align, "right", StringComparison.OrdinalIgnoreCase) :
				!string.IsNullOrEmpty(Name) && Name == currentUserName;

		/// <summary>
		/// Is the message from a group chat (needed to build file URLs and cache paths).
		/// </summary>
		[JsonIgnore]
		public bool IsGroupChat { get; set; }

		/// <summary>
		/// Identifier of a message created on this device, <c>null</c> for server messages.
		/// </summary>
		[JsonIgnore]
		public string LocalId { get; set; }

		/// <summary>
		/// Path of a local file being sent (image preview and retry).
		/// </summary>
		[JsonIgnore]
		public string LocalFilePath { get; set; }

		[JsonIgnore]
		public bool IsImageMessage => IsImage == true;

		[JsonIgnore]
		public bool IsFileMessage => IsFile == true;

		[JsonIgnore]
		public bool IsAttachment => IsImageMessage || IsFileMessage;

		[JsonIgnore]
		public DateTime LocalTime =>
			Time.Kind == DateTimeKind.Local ?
				Time :
				DateTime.SpecifyKind(Time, DateTimeKind.Utc).ToLocalTime();

		/// <summary>
		/// True когда сообщение - обычный текст, без вложений.
		/// Используется для показа/скрытия textLabel в ячейке чата.
		/// </summary>
		[JsonIgnore]
		public bool IsPlainText => !IsAttachment;

		string _linksCheckedText;
		bool _hasLinks;

		/// <summary>
		/// Does the text contain links (checked once per text).
		/// </summary>
		[JsonIgnore]
		public bool HasLinks
		{
			get
			{
				if (!ReferenceEquals(_linksCheckedText, Text))
				{
					_hasLinks = EduCATS.Pages.Chat.Services.MessageLinkParser.GetFirstUrl(Text) != null;
					_linksCheckedText = Text;
				}

				return _hasLinks;
			}
		}

		[JsonIgnore]
		public bool HasNoLinks => !HasLinks;

		/// <summary>
		/// "имя_файла.pdf (123.4 KB)" для чипа файла в UI.
		/// </summary>
		[JsonIgnore]
		public string FileDisplayText
		{
			get
			{
				var fileName = FileName;
				return string.IsNullOrEmpty(FileSize) ? fileName : $"{fileName} ({FileSize})";
			}
		}

		/// <summary>
		/// File names are never that long: longer content is the file itself.
		/// </summary>
		const int _maxFileNameLength = 260;

		/// <summary>
		/// Old messages carry the file itself (base64) in <see cref="FileContent"/>
		/// and its name in <see cref="Text"/>; new ones - the uploaded file name.
		/// </summary>
		[JsonIgnore]
		public bool HasInlineFile => FileContent?.Length > _maxFileNameLength;

		/// <summary>
		/// Attachment file name.
		/// </summary>
		[JsonIgnore]
		public string FileName =>
			HasInlineFile || string.IsNullOrEmpty(FileContent) ?
				(string.IsNullOrWhiteSpace(Text) ? "file" : Text) :
				FileContent;

		MessageSendStatus _status;

		/// <summary>
		/// Sending status (own messages only).
		/// </summary>
		[JsonIgnore]
		public MessageSendStatus Status
		{
			get => _status;
			set
			{
				if (setProperty(ref _status, value))
				{
					OnPropertyChanged(nameof(IsFailed));
					OnPropertyChanged(nameof(IsUploading));
				}
			}
		}

		[JsonIgnore]
		public bool IsFailed => Status == MessageSendStatus.Failed;

		double _uploadProgress;

		/// <summary>
		/// Attachment upload progress from 0 to 1.
		/// </summary>
		[JsonIgnore]
		public double UploadProgress
		{
			get => _uploadProgress;
			set => setProperty(ref _uploadProgress, value);
		}

		[JsonIgnore]
		public bool IsUploading => Status == MessageSendStatus.Sending && IsAttachment;

		LinkPreviewModel _linkPreview;

		/// <summary>
		/// Preview of the first link in the text (loaded lazily).
		/// </summary>
		[JsonIgnore]
		public LinkPreviewModel LinkPreview
		{
			get => _linkPreview;
			set
			{
				if (setProperty(ref _linkPreview, value))
				{
					OnPropertyChanged(nameof(HasLinkPreview));
				}
			}
		}

		[JsonIgnore]
		public bool HasLinkPreview => LinkPreview != null;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		bool setProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
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
