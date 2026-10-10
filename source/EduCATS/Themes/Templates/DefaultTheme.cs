using EduCATS.Themes.Interfaces;

namespace EduCATS.Themes.Templates
{
	/// <summary>
	/// <see cref="ITheme"/> implementation (Default light theme).
	/// </summary>
	/// <remarks>
	/// Used to set values for a theme.
	/// </remarks>
	public class DefaultTheme : ITheme
	{
		const string _greyColor = "#808080";
		const string _whiteColor = "#FFFFFF";
		const string _baseBlueColor = "#3F51B5";
		const string _lightGreyColor = "#F7F5F3";
		const string _blackColor = "#000000";

		virtual public string SubGroupText => _blackColor;
		virtual public string BaseAppColor => _baseBlueColor;
		virtual public string BaseBlockColor => _whiteColor;
		virtual public string BaseSectionTextColor => _blackColor;
		virtual public string BaseActivityIndicatorColorIOS => _blackColor;
		virtual public string BaseActivityIndicatorColorAndroid => _blackColor;
		virtual public string BasePickerTextColor => _blackColor;
		virtual public string BaseArrowForwardIcon => "icon_forward.png";
		virtual public string BaseCloseIcon => "icon_close.png";
		virtual public string BaseLogoImage => "image_logo_rounded.png";
		virtual public string BaseHeadphonesIcon => "icon_headphones_blue.png";
		virtual public string BaseHeadphonesCancelIcon => "icon_headphones_cancel_blue.png";
		virtual public string BaseNoDataTextColor => _blackColor;
		virtual public string BaseLinksColor => _baseBlueColor;

		virtual public string AppBackgroundColor => _lightGreyColor;
		virtual public string AppStatusBarBackgroundColor => _baseBlueColor;
		virtual public string AppNavigationBarBackgroundColor => _whiteColor;

		virtual public string RoundedListViewBackgroundColor => _whiteColor;

		virtual public string LoginBackground1Image => "image_background_1.jpg";
		virtual public string LoginBackground2Image => "image_background_2.jpg";
		virtual public string LoginBackground3Image => "image_background_3.jpg";
		virtual public string LoginMascotImage => "image_mascot.png";
		virtual public string LoginMascotTailImage => "image_mascot_tail.png";
		virtual public string LoginShowPasswordImage => "icon_show_password.png";
		virtual public string LoginEntryBackgroundColor => _whiteColor;
		virtual public string LoginButtonBackgroundColor => _baseBlueColor;
		virtual public string LoginButtonTextColor => _whiteColor;
		virtual public string LoginSettingsColor => _whiteColor;

		virtual public string MainSelectedTabColor => _baseBlueColor;
		virtual public string MainUnselectedTabColor => _greyColor;
		virtual public string MainTodayIcon => "icon_today.png";
		virtual public string MainLearningIcon => "icon_learning.png";
		virtual public string MainStatisticsIcon => "icon_stats.png";
		virtual public string MainSettingsIcon => "icon_settings.png";

		virtual public string MainChatIcon => "icon_chat.png";

		virtual public string TodayCalendarBackgroundColor => _whiteColor;
		virtual public string TodaySubjectBackgroundColor => _whiteColor;
		virtual public string TodayNewsItemBackgroundColor => _whiteColor;
		virtual public string TodayNewsListBackgroundColor => _lightGreyColor;
		virtual public string TodayNewsTitleColor => "#222222";
		virtual public string TodayNewsSubjectColor => _greyColor;
		virtual public string TodayNewsDateColor => _greyColor;
		virtual public string TodayNewsDateIconColor => "#ADABAA";
		virtual public string TodayNewsDateIcon => "icon_clock.png";
		virtual public string TodaySelectedTodayDateColor => _baseBlueColor;
		virtual public string TodaySelectedAnotherDateColor => _greyColor;
		virtual public string TodayNotSelectedDateColor => "Transparent";
		virtual public string TodaySelectedDateTextColor => _whiteColor;
		virtual public string TodayNotSelectedDateTextColor => _blackColor;
		virtual public string TodayCalendarBaseTextColor => _blackColor;
		virtual public string TodayCalendarSubjectTextColor => _blackColor;

		virtual public string NewsTextColor => _blackColor;


		virtual public string StatisticsChartLabsColor => "#D32F2F";
		virtual public string StatisticsChartPractColor => "#FFA500";
		virtual public string StatisticsChartTestsColor => "#1976D2";
		virtual public string StatisticsChartCourseColor => "#7F00FF";
		virtual public string StatisticsChartRatingColor => "#3F51B5";
		virtual public string StatisticsBoxTextColor => _whiteColor;
		virtual public string StatisticsExpandableTextColor => _greyColor;
		virtual public string StatisticsDetailsTitleColor => _blackColor;
		virtual public string StatisticsDetailsColor => _greyColor;
		virtual public string StatisticsDetailsSeparatorColor => "#FCFAF8";
		virtual public string StatisticsDetailsResultsColor => _baseBlueColor;
		virtual public string StatisticsDetailsNameColor => _blackColor;
		virtual public string StatisticsExpandIcon => "icon_expand.png";
		virtual public string StatisticsCollapseIcon => "icon_collapse.png";
		virtual public string StatisticsCalendarIcon => "icon_calendar.png";
		virtual public string StatisticsCommentIcon => "icon_comment.png";
		virtual public string StatisticsBaseTitleColor => _blackColor;
		virtual public string StatisticsBaseRatingTextColor => _blackColor;

		virtual public string LearningCardTextColor => _whiteColor;

		virtual public string LearningCardTestsImage => "image_test_square.jpg";
		virtual public string LearningCardEemcImage => "image_book_square.jpg";
		virtual public string LearningCardFilesImage => "image_file_square.jpg";
		virtual public string LearningCardAdaptiveImage => "image_circuit_square.jpg";

		virtual public string TestingTitleColor => _blackColor;
		virtual public string TestingDescriptionColor => _greyColor;
		virtual public string TestingHeaderImage => "image_test_rectangle.jpg";

		virtual public string TestPassingButtonTextColor => _whiteColor;
		virtual public string TestPassingEntryColor => _whiteColor;
		virtual public string TestPassingAnswerColor => _blackColor;
		virtual public string TestPassingArrowUpIcon => "icon_arrow_up.png";
		virtual public string TestPassingArrowDownIcon => "icon_arrow_down.png";
		virtual public string TestPassingQuestionColor => _blackColor;
		virtual public string TestPassingSelectionColor => _baseBlueColor;
		virtual public string TestPassingUnselectedColor => _blackColor;

		virtual public string TestResultsAnswerTextColor => _blackColor;
		virtual public string TestResultsCorrectAnswerColor => "#00AA55";
		virtual public string TestResultsNotCorrectAnswerColor => "#E63022";
		virtual public string TestResultsRatingColor => _blackColor;
		

		virtual public string EemcHeaderImage => "image_book_rectangle.jpg";
		virtual public string EemcBackButtonColor => _baseBlueColor;
		virtual public string EemcBackButtonTextColor => _whiteColor;
		virtual public string EemcDirectoryActiveIcon => "icon_directory_active.png";
		virtual public string EemcDirectoryInactiveIcon => "icon_directory_inactive.png";
		virtual public string EemcDocumentActiveIcon => "icon_document_pdf_active.png";
		virtual public string EemcDocumentInactiveIcon => "icon_document_pdf_inactive.png";
		virtual public string EemcDocumentTestActiveIcon => "icon_document_test_active.png";
		virtual public string EemcDocumentTestInactiveIcon => "icon_document_test_inactive.png";
		virtual public string EemcItemTitleColor => _blackColor;

		virtual public string FilesHeaderImage => "image_file_rectangle.jpg";
		virtual public string FilesTitleColor => _blackColor;
		virtual public string FilesSizeColor => _greyColor;
		virtual public string FilesDownloadedIcon => "icon_download.png";

		virtual public string RecommendationsHeaderImage => "image_circuit_rectangle.jpg";
		virtual public string RecommendationsTitleColor => _blackColor;

		virtual public string SettingsServerIcon => "icon_settings_server.png";
		virtual public string SettingsLanguageIcon => "icon_settings_language.png";
		virtual public string SettingsThemeIcon => "icon_settings_themes.png";
		virtual public string SettingsAboutIcon => "icon_settings_about.png";
		virtual public string SettingsFontIcon => "icon_settings_font.png";
		virtual public string SettingsLogoutIcon => "icon_settings_logout.png";
		virtual public string SettingsGroupUserColor => _blackColor;
		virtual public string SettingsTitleColor => _blackColor;
		virtual public string SettingsProfileColor => _whiteColor;
		virtual public string SettingsTableColor => _whiteColor;
		virtual public string SettingsProfileLabelColor => _greyColor;
		virtual public string ProfileNameIcon => "icon_profile_name.png";
		virtual public string ProfileLoginIcon => "icon_profile_login.png";
		virtual public string ProfileEmailIcon => "icon_profile_email.png";
		virtual public string ProfilePhoneIcon => "icon_profile_phone.png";
		virtual public string ProfileSocialMediaIcon => "icon_profile_social_media.png";
		virtual public string ProfileGroupIcon => "icon_profile_group.png";
		virtual public string ProfileAboutIcon => "icon_profile_about.png";


		virtual public string CheckboxIcon => "icon_checkmark.png";
		virtual public string CheckboxDescriptionColor => _greyColor;

		virtual public string SwitchFrameTextColor => _blackColor;
		virtual public string SwitchFrameDescriptionColor => _greyColor;

		virtual public string AboutTextColor => _blackColor;
		virtual public string AboutButtonTextColor => _whiteColor;
		virtual public string AboutButtonBackgroundColor => _baseBlueColor;
	}
}

