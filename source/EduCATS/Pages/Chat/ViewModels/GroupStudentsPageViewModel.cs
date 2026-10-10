using EduCATS.Helpers.Forms;
using EduCATS.Pages.Chat.Models;
using EduCATS.Pages.Chat.Services;
using System;
using Nyxbull.Plugins.CrossLocalization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EduCATS.Pages.Chat.ViewModels
{
	/// <summary>
	/// ViewModel попапа "Список студентов", открываемого по нажатию
	/// icon_students в шапке группового чата.
	/// </summary>
	public class GroupStudentsPageViewModel : ViewModel
	{
		readonly IPlatformServices _services;
		readonly int _groupId;

		public GroupStudentsPageViewModel(IPlatformServices services, int groupId, string subtitle)
		{
			_services = services;
			_groupId = groupId;
			Subtitle = subtitle;
			Students = new ObservableCollection<StudentItemModel>();

			_ = load();
		}

		public string Subtitle { get; }

		ObservableCollection<StudentItemModel> _students;

		public ObservableCollection<StudentItemModel> Students
		{
			get { return _students; }
			set { SetProperty(ref _students, value); }
		}

		bool _isLoading;

		public bool IsLoading
		{
			get { return _isLoading; }
			set { SetProperty(ref _isLoading, value); }
		}

		bool _isEmpty;

		public bool IsEmpty
		{
			get { return _isEmpty; }
			set { SetProperty(ref _isEmpty, value); }
		}

		async Task load()
		{
			IsLoading = true;

			try
			{
				var students = await ChatApiService.GetStudentsByGroupId(_groupId)
					?? new List<StudentItemModel>();

				Students = new ObservableCollection<StudentItemModel>(
					students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName));

				if (ChatApiService.IsError)
				{
					_services.Dialogs.ShowError(CrossLocalization.Translate("chat_students_load_error"));
				}

				IsEmpty = Students.Count == 0;
			}
			finally
			{
				IsLoading = false;
			}
		}
	}
}