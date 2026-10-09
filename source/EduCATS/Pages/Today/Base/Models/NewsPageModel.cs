using EduCATS.Data.Models;
using EduCATS.Helpers.Date;

namespace EduCATS.Pages.Today.Base.Models
{
	public class NewsPageModel
	{
		public NewsPageModel(NewsModel newsModel, string subjectColor)
		{
			setDefaultProps(newsModel);
			setSubjectColor(subjectColor);
		}

		void setDefaultProps(NewsModel newsModel)
		{
			if (newsModel != null) {
				Id = newsModel.Id;
				Title = newsModel.Title;
				Body = newsModel.Body;
				Date = newsModel.Date;

				if (newsModel.Subject != null) {
					SubjectName = newsModel.Subject.Name;
				}
			}
		}

		void setSubjectColor(string subjectColor)
		{
			if (subjectColor != null) {
				SubjectColor = subjectColor;
			}
		}

		public int Id { get; set; }
		public string Body { get; set; }
		public string Title { get; set; }
		public string SubjectName { get; set; }
		public string SubjectColor { get; set; }

		// Formatted once on set: the getter is hit by bindings on every item recycle while scrolling.
		string date;
		public string Date {
			get {
				return date;
			}
			set {
				var unixDoble = DateHelper.GetUnixFromString(value);
				var unixString = DateHelper.Convert13DigitsUnixToDateTime(unixDoble).ToLocalTime();
				date = unixString.ToString(DateHelper.DateTime);
			}
		}
	}
}

