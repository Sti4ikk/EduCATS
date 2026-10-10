using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using EduCATS.Constants;
using EduCATS.Data.Models;
using EduCATS.Data.Models.Calendar;
using EduCATS.Data.Models.User;
using EduCATS.Networking;
using EduCATS.Networking.Models.Login;
using EduCATS.Networking.Models.SaveMarks;
using EduCATS.Networking.Models.SaveMarks.Labs;
using EduCATS.Networking.Models.SaveMarks.LabSchedule;
using EduCATS.Networking.Models.SaveMarks.Practicals;
using EduCATS.Networking.Models.Testing;
using EduCATS.Pages.Parental.FindGroup.Models;
using EduCATS.Pages.Statistics.Results.Models;

namespace EduCATS.Data
{
	/// <summary>
	/// Wrapper for API calls.
	/// Fetches data, handles and caches it.
	/// </summary>
	public static partial class DataAccess
	{
		public static string Username { get; private set; }

		/// <summary>
		/// Authorize.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <returns>User data.</returns>
		public async static Task<DataResult<UserModel>> Login(string username, string password)
		{
			var dataAccess = new DataAccess<UserModel>("login_error", loginCallback(username, password));
			return await GetSingleData(dataAccess);
		}

		public async static Task<DataResult<SecondUserModel>> GetAccountData()
		{
			var dataAccess = new DataAccess<SecondUserModel>("login_error", getAccountDataCallback());
			return await GetSingleData(dataAccess);
		}

		public async static Task<DataResult<TokenModel>> GetToken(string username, string password)
		{
			var dataAccess = new DataAccess<TokenModel>("login_error", getTokenCallback(username, password));
			return await GetSingleData(dataAccess);
		}

		public async static Task<DataResult<DeleteAccountModel>> DeleteAccount()
		{
			var dataAccess = new DataAccess<DeleteAccountModel>(
				"base_error", deleteAccountCallback());
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch profile information.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <returns>User profile data.</returns>
		public async static Task<DataResult<UserProfileModel>> GetProfileInfo(string username)
		{
			Username = username;
			var dataAccess = new DataAccess<UserProfileModel>(
				"login_user_profile_error", getProfileCallback(username), GlobalConsts.DataProfileKey);
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch news.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <returns>News data.</returns>
		public async static Task<DataResult<List<NewsModel>>> GetNews(string username)
		{
			var dataAccess = new DataAccess<NewsModel>(
				"today_news_load_error", getNewsCallback(username), GlobalConsts.DataGetNewsKey);
			return await GetListData(dataAccess);
		}

		/// <summary>
		/// Fetch subjects.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <returns>Subjects data.</returns>
		public async static Task<DataResult<List<SubjectModel>>> GetProfileInfoSubjects(string username)
		{
			var dataAccess = new DataAccess<SubjectModelTest>(
				"today_subjects_error", getSubjectsCallback(username), GlobalConsts.DataGetSubjectsKey);
			return (await GetSingleData(dataAccess)).Map(model => model.Subjects);
		}

		/// <summary>
		/// Fetch subject modules.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <returns>Subject modules data.</returns>
		public async static Task<DataResult<List<SubjectModuleModel>>> GetSubjectModules(int subjectId)
		{
			var dataAccess = new DataAccess<SubjectModuleModel>(
				"stats_marks_error",
				getSubjectModulesCallback(subjectId),
				GetKey(GlobalConsts.DataGetSubjectModulesKey, subjectId));
			return await GetListData(dataAccess);
		}

		/// <summary>
		/// Fetch subject lecturers info.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <returns>Lecturers data.</returns>
		public async static Task<DataResult<InfoLecturesModel>> GetInfoLectures(int subjectId)
		{
			var dataAccess = new DataAccess<InfoLecturesModel>(
				"today_subjects_error", getInfoLecturesCallback(subjectId),
				GetKey(GlobalConsts.DataGetInfoLecturesKey, subjectId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch calendar data.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <returns>Calendar data.</returns>
		public async static Task<DataResult<CalendarModel>> GetProfileInfoCalendar(string username)
		{
			var dataAccess = new DataAccess<CalendarModel>(
				"today_calendar_error", getCalendarCallback(username), GlobalConsts.DataGetCalendarKey);
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch schedule calendar data.
		/// </summary>
		/// <param name="date">Date.</param>
		/// <returns>Calendar data.</returns>
		public async static Task<DataResult<CalendarSubjectModelTest>> GetSchedule(string date)
		{
			return await GetSchedule(date, date);
		}

		/// <summary>
		/// Fetch schedule calendar data.
		/// </summary>
		/// <param name="dateStart">Start date.</param>
		/// <param name="dateEnd">End date.</param>
		/// <returns>Calendar data.</returns>
		public async static Task<DataResult<CalendarSubjectModelTest>> GetSchedule(string dateStart, string dateEnd)
		{
			var dataAccess = new DataAccess<CalendarSubjectModelTest>(
				"today_calendar_error", getScheduleCallback(dateStart, dateEnd),
				GetKey(GlobalConsts.DataGetCalendarKey, dateStart, dateEnd));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch diploma project consultations.
		/// </summary>
		/// <param name="count">Items count.</param>
		/// <param name="page">Page number.</param>
		/// <returns>Consultations data.</returns>
		public async static Task<DataResult<DiplomProjectConsultationModel>> GetDiplomProjectConsultation(
			int count = 1000, int page = 1)
		{
			var dataAccess = new DataAccess<DiplomProjectConsultationModel>(
				"today_calendar_error", getDiplomProjectConsultationCallback(count, page));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch course project consultations.
		/// </summary>
		/// <param name="count">Items count.</param>
		/// <param name="page">Page number.</param>
		/// <returns>Consultations data.</returns>
		public async static Task<DataResult<CourseProjectConsultationModel>> GetCourseProjectConsultation(
			int count = 1000, int page = 1)
		{
			var dataAccess = new DataAccess<CourseProjectConsultationModel>(
				"today_calendar_error", getCourseProjectConsultationCallback(count, page));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch profile info by id.
		/// </summary>
		/// <param name="userId">User id.</param>
		/// <returns>User profile data.</returns>
		public async static Task<DataResult<UserProfileByIdModel>> GetProfileInfoById(int userId)
		{
			var dataAccess = new DataAccess<UserProfileByIdModel>(
				"today_calendar_error", getProfileInfoByIdCallback(userId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch students statistics.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <param name="groupId">Group ID.</param>
		/// <returns>Students statistics data.</returns>
		public async static Task<DataResult<StatsModel>> GetStudentsStatistics(int subjectId, int groupId)
		{
			var dataAccess = new DataAccess<StatsModel>(
				"stats_marks_error", getStudentsStatsCallback(subjectId, groupId),
				GetKey(GlobalConsts.DataGetStudentsStatsKey, subjectId, groupId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch statistics.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <param name="groupId">Group ID.</param>
		/// <returns>Statistics data.</returns>
		public async static Task<DataResult<StatsModel>> GetStatistics(int subjectId, int groupId)
		{
			var dataAccess = new DataAccess<StatsModel>(
				"stats_marks_error", getStatsCallback(subjectId, groupId),
				GetKey(GlobalConsts.DataGetMarksKey, subjectId, groupId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch student summary statistics.
		/// </summary>
		/// <returns>Student summary statistics data.</returns>
		public async static Task<DataResult<StudentStatisticsSummaryModel>> GetStudentStatisticsSummary()
		{
			var dataAccess = new DataAccess<StudentStatisticsSummaryModel>(
				"stats_marks_error", getStudentStatisticsSummaryCallback());
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch teacher summary statistics.
		/// </summary>
		/// <returns>Teacher summary statistics data.</returns>
		public async static Task<DataResult<TeacherStatisticsSummaryModel>> GetTeacherStatisticsSummary()
		{
			var dataAccess = new DataAccess<TeacherStatisticsSummaryModel>(
				"stats_marks_error", getTeacherStatisticsSummaryCallback());
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch test statistics.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <param name="groupId">Group ID.</param>
		/// <returns>Statistics data.</returns>
		public async static Task<DataResult<LabsVisitingList>> GetTestStatistics(int subjectId, int groupId)
		{
			var dataAccess = new DataAccess<LabsVisitingList>(
				"stats_marks_error", getTestStatsCallback(subjectId, groupId),
				GetKey(GlobalConsts.DataGetLabsVisitingKey, subjectId, groupId));
			return await GetSingleData(dataAccess);
		}
	
		public async static Task<DataResult<TakedLabs>> GetPractTest(int subjectId, int groupId)
		{
			var dataAccess = new DataAccess<TakedLabs>(
				"stats_marks_error", getTestPractScheduleCallbak(subjectId, groupId),
				GetKey(GlobalConsts.DataGetPractsScheduleKey, subjectId, groupId));
			return await GetSingleData(dataAccess);
		}

		public async static Task<DataResult<Practs>> GetPracticals(int subjectId)
		{
			var dataAccess = new DataAccess<Practs>(
				"stats_marks_error", getTestPractScheduleCallbak(subjectId),
				GetKey(GlobalConsts.DataGetPractsKey, subjectId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch statistics.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <param name="groupId">Group ID.</param>
		/// <returns>Statistics data.</returns>
		public async static Task<DataResult<LabsVisitingList>> GetTestPracticialStatistics(int subjectId, int groupId)
		{
			var dataAccess = new DataAccess<LabsVisitingList>(
				"stats_marks_error", getTestPracticialStatsCallback(subjectId, groupId),
				GetKey(GlobalConsts.DataGetPractsVisitingKey, subjectId, groupId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch groups.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <returns>Group data.</returns>
		public async static Task<DataResult<GroupModel>> GetOnlyGroups(int subjectId)
		{
			var dataAccess = new DataAccess<GroupModel>(
				"groups_fetch_error", getGroupsCallback(subjectId),
				GetKey(GlobalConsts.DataGetGroupsKey, subjectId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch groups data.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <returns>Group data.</returns>
		public async static Task<DataResult<List<GroupItemModel>>> GetGroupsData()
		{
			var dataAccess = new DataAccess<GroupItemModel>(
				"groups_fetch_error", getGroupsDataCallback());
			return await GetListData(dataAccess);
		}

		public async static Task<DataResult<LecturesModel>> GetLecturesTest(int subjectId, int groupId)
		{
			var dataAccess = new DataAccess<LecturesModel>(
				"lectures_fetch_error", getLecturesCallbackTest(subjectId, groupId),
				GetKey(GlobalConsts.DataGetLecturesEducatsKey, subjectId, groupId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch laboratory works data.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <param name="groupId">Group ID.</param>
		/// <returns>Laboratory works data.</returns>
		public async static Task<DataResult<LabsModel>> GetLabs(int subjectId, int groupId)
		{
			var dataAccess = new DataAccess<LabsModel>(
				"labs_fetch_error", getLabsCallback(subjectId, groupId),
				GetKey(GlobalConsts.DataGetLabsKey, subjectId, groupId));
			return await GetSingleData(dataAccess);
		}

		public async static Task<DataResult<Laboratories>> GetLabs(int subjectId)
		{
			var dataAccess = new DataAccess<Laboratories>(
				"labs_fetch_error", getLabsCallback(subjectId),
				GetKey(GlobalConsts.DataGetLabsKey, subjectId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch laboratory works data.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <param name="groupId">Group ID.</param>
		/// <returns>Laboratory works data.</returns>
		public async static Task<DataResult<TakedLabs>> GetLabsTest(int subjectId, int groupId)
		{
			var dataAccess = new DataAccess<TakedLabs>(
				"labs_fetch_error", getTestLabsCallback(subjectId, groupId),
				GetKey(GlobalConsts.DataGetLabsScheduleKey, subjectId, groupId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch lectures data.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <param name="groupId">Group ID.</param>
		/// <returns>Lectures data.</returns>
		public async static Task<DataResult<LecturesModel>> GetLectures(int subjectId, int groupId)
		{
			var dataAccess = new DataAccess<LecturesModel>(
				"lectures_fetch_error", getLecturesCallback(subjectId, groupId),
				GetKey(GlobalConsts.DataGetLecturesKey, subjectId, groupId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch tests.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <param name="userId">User ID.</param>
		/// <returns>List of test data.</returns>
		public async static Task<DataResult<List<TestModel>>> GetAvailableTests(int subjectId, int userId)
		{
			var dataAccess = new DataAccess<TestModel>(
				"testing_get_tests_error", getTestsCallback(subjectId, userId),
				GetKey(GlobalConsts.DataGetTestsKey, subjectId, userId));
			return await GetListData(dataAccess);
		}

		/// <summary>
		/// Get test information.
		/// </summary>
		/// <param name="testId">Test ID.</param>
		/// <returns>Test details data.</returns>
		public async static Task<DataResult<TestDetailsModel>> GetTest(int testId)
		{
			var dataAccess = new DataAccess<TestDetailsModel>("get_test_error", getTestCallback(testId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch next question.
		/// </summary>
		/// <param name="testId">Test ID.</param>
		/// <param name="questionNumber">Question number.</param>
		/// <param name="userId">User ID.</param>
		/// <returns>Test question data.</returns>
		public async static Task<DataResult<TestQuestionModel>> GetNextQuestion(int testId, int questionNumber, int userId)
		{
			var dataAccess = new DataAccess<TestQuestionModel>(
				"get_test_question_error", getNextQuestionCallback(testId, questionNumber, userId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Answer question.
		/// </summary>
		/// <param name="answer">Answer data.</param>
		/// <returns>String. <c>"Ok"</c>, for example.</returns>
		public async static Task<DataResult<object>> AnswerQuestionAndGetNext(TestAnswerPostModel answer)
		{
			var dataAccess = new DataAccess<object>("answer_question_error", answerQuestionCallback(answer));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch test answers.
		/// </summary>
		/// <param name="userId">User ID.</param>
		/// <param name="testId">Test ID.</param>
		/// <returns>List of results data.</returns>
		public async static Task<DataResult<List<TestResultsModel>>> GetUserAnswers(int userId, int testId)
		{
			var dataAccess = new DataAccess<TestResultsModel>(
				"test_results_error", getTestAnswersCallback(userId, testId),
				GetKey(GlobalConsts.DataGetTestAnswersKey, userId, testId));
			return await GetListData(dataAccess);
		}

		public async static Task<DataResult<ExtendedTestResultModel>> GetUserAnswers(int testId)
		{
			var dataAccess = new DataAccess<ExtendedTestResultModel>(
				"test_results_error", getTestAnswersCallback(testId),
				GetKey(GlobalConsts.DataGetTestAnswersKey, testId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch Electronic Educational Methodological Complexes
		/// root concepts.
		/// </summary>
		/// <param name="userId">User ID.</param>
		/// <param name="subjectId">Subject ID.</param>
		/// <returns>Root concept data.</returns>
		public async static Task<DataResult<RootConceptModel>> GetRootConcepts(string userId, string subjectId)
		{
			var dataAccess = new DataAccess<RootConceptModel>(
				"eemc_root_concepts_error", getRootConceptsCallback(subjectId),
				GetKey(GlobalConsts.DataGetRootConceptKey, subjectId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch Electronic Educational Methodological Complexes
		/// concept tree.
		/// </summary>
		/// <param name="elementId">Root element ID.</param>
		/// <returns>Concept data.</returns>
		public async static Task<DataResult<ConceptModel>> GetConceptTree(int elementId)
		{
			var dataAccess = new DataAccess<ConceptModel>(
				"eemc_concept_tree_error", getConceptTreeCallback(elementId),
				GetKey(GlobalConsts.DataGetConceptTreeKey, elementId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch Electronic Educational Methodological Complexes
		/// concept cascade.
		/// </summary>
		/// <param name="elementId">Root element ID.</param>
		/// <returns>Concept data.</returns>
		public async static Task<DataResult<ConceptModelTest>> GetConceptCascade(int elementId)
		{
			var dataAccess = new DataAccess<ConceptModelTest>(
				"eemc_concept_tree_error", getConceptCascadeCallback(elementId),
				GetKey(GlobalConsts.DataGetConceptCascadeKey, elementId));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch files.
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <returns>Files data.</returns>
		public async static Task<DataResult<FilesModel>> GetFiles(int subjectId)
		{
			var dataAccess = new DataAccess<FilesModel>(
				"files_fetch_error", getFilesCallback(subjectId),
				GetKey(GlobalConsts.DataGetFilesKey, subjectId));
			return await GetSingleData(dataAccess);
		}

		public async static Task<DataResult<FilesModelTest>> GetFilesTest(int subjectId)
		{
			var dataAccess = new DataAccess<FilesModelTest>(
				"files_fetch_error", getFilesCallback(subjectId),
				GetKey(GlobalConsts.DataGetFilesKey, subjectId));
			return await GetSingleData(dataAccess);
		}

		public async static Task<DataResult<List<FileDetailsModelTest>>> GetDetailsFilesTest(IEnumerable<string> values)
		{
			var dataAccess = new DataAccess<FileDetailsModelTest>(
				"files_fetch_error", getFilesDetailsCallback(values));
			return await GetListData(dataAccess);
		}

		/// <summary>
		/// Load goup info by groupName
		/// </summary>
		/// <param name="groupName">group Name</param>
		/// <returns></returns>
		public async static Task<DataResult<GroupInfo>> GetGroupInfo(string groupName)
		{
			var dataAccess = new DataAccess<GroupInfo>(
				"Error", getGroupInfoCallback(groupName));
			return await GetSingleData(dataAccess);
		}

		/// <summary>
		/// Fetch recommendations (adaptive learning).
		/// </summary>
		/// <param name="subjectId">Subject ID.</param>
		/// <param name="userId">User ID.</param>
		/// <returns>List of recommendations data.</returns>
		public async static Task<DataResult<List<RecommendationModel>>> GetRecommendations(int subjectId, int userId)
		{
			var dataAccess = new DataAccess<RecommendationModel>(
				"recommendations_fetch_error", getRecommendationsCallback(subjectId, userId),
				GetKey(GlobalConsts.DataGetRecommendationsKey, subjectId, userId));
			return await GetListData(dataAccess);
		}
	}
}

