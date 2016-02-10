using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Slack.Webhooks;
using Slack2FAChecker.Model;

namespace Slack2FAChecker
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			var slack2FAChecker = new Slack2FAChecker();
			var result = slack2FAChecker.Slack().Result;
		}
	}

	internal class Slack2FAChecker
	{
		public async Task<int> Slack()
		{
			var url = ConfigurationManager.AppSettings["SlackApiUrl"];
			var token = ConfigurationManager.AppSettings["SlackApiToken"];

			var client = new HttpClient();
			var response = await client.GetAsync(url + token);
			var users = await response.Content.ReadAsAsync<UserList>();

			var no2fauser = users.members.Where(x => x.has_2fa == false && x.deleted == false && x.name != "slackbot").ToList();
			if (no2fauser.Any())
			{
				SlackPost("2FA 無効ユーザ\n" + string.Join(", ", no2fauser.Select(x => x.name)));
			}
			else
			{
				SlackPost("2FA が無効のユーザはいません！セキュアデス！");
			}

			return 1;
		}

		private void SlackPost(string message)
		{
			var client = new SlackClient(ConfigurationManager.AppSettings["SlackWebHookUrl"]);
			var slackMessage = new SlackMessage
			{
				Channel = "#test",
				Text = message,
				IconEmoji = ":slack:",
				Username = "Slack2FAChecker"
			};
			client.Post(slackMessage);
		}
	}
}
