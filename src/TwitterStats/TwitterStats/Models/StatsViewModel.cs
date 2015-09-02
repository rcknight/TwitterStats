using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using LinqToTwitter;

namespace TwitterStats.Models
{
    public class StatsViewModel
    {

        public string UserName { get; private set; }
        public int Following { get; private set; }
        public int Followers { get; private set; }
        public int TweetsToday { get; private set; }
        public int TweetsAllTime { get; private set; }
        public decimal ExpectedWins { get; private set; }
        private List<Status> AllTweets { get; set; }
        public List<Object> DailyTweets { get; private set; }
        public List<Object> DailyCompetitors { get; private set; } 
        private ITwitterAuthorizer Auth { get; set; }

        private static String[] Users = new[]
         { "RichardK1986", "Compasauraus", "Mr_Richio", "RKCompetitions", "CompWinner86", "CompoRick"};

        public StatsViewModel()
        {
            UserName = String.Format("{0} Users", Users.Count());
            AllTweets = new List<Status>();
            Auth = new SingleUserAuthorizer()
            {
                Credentials = new SingleUserInMemoryCredentials()
                {
                    ConsumerKey =
                        ConfigurationManager.AppSettings["TWITTER_CONSUMER_KEY"],
                    ConsumerSecret =
                        ConfigurationManager.AppSettings["TWITTER_CONSUMER_SECRET"],
                    TwitterAccessToken =
                        ConfigurationManager.AppSettings["TWITTER_ACCESS_TOKEN"],
                    TwitterAccessTokenSecret =
                        ConfigurationManager.AppSettings["TWITTER_ACCESS_TOKEN_SECRET"]
                }
            };
            Update();
        }

        private object UpdateLock = new Object();

        public void Update()
        {
            lock (UpdateLock)
            {
                //load tweets
                UpdateTweets();
                //user stuff
                var allDetails = Users.Select(GetUserDetails).ToList();
                Following = allDetails.Sum(d => d.FriendsCount);
                Followers = allDetails.Sum(d => d.FollowersCount);
                TweetsAllTime = allDetails.Sum(d => d.StatusesCount);

                //Tweet based stuff
                TweetsToday = AllTweets.Count(t => t.CreatedAt.ToUniversalTime() > DateTime.Today.ToUniversalTime());
                var retweets =
                    AllTweets.Where(
                        t =>
                        t.CreatedAt.ToUniversalTime() < DateTime.Today.ToUniversalTime().AddDays(-1) &&
                        t.RetweetedStatus != null && t.RetweetedStatus.StatusID != null &&
                        t.RetweetedStatus.RetweetCount > 0).ToList();
                ExpectedWins =
                    retweets.Sum(tweet => (decimal) 1/(decimal) tweet.RetweetedStatus.RetweetCount);

                UpdateDailyCounts();
            }
        }

        private void UpdateDailyCounts()
        {
            var days = AllTweets.GroupBy(t => t.CreatedAt.Date.ToString("yyyy-MM-dd")).Reverse();

            DailyTweets = new List<object>();
            DailyCompetitors = new List<object>();

            foreach (var day in days)
            {
                var tweets = day.Where(t => t.RetweetedStatus == null || t.RetweetedStatus.StatusID == null).ToList();
                var retweets = day.Where(t => t.RetweetedStatus != null && t.RetweetedStatus.StatusID != null).ToList();

                var competitors = retweets.Sum(rt => rt.RetweetedStatus.RetweetCount) / retweets.Count();

                DailyTweets.Add(new { date=day.Key, tweets=tweets.Count(), retweets=retweets.Count() });
                DailyCompetitors.Add(new { date = day.Key, competitors, label = day.Key });
            }
        }

        private User GetUserDetails(string userName)
        {
            using (var twitter = new TwitterContext(Auth))
            {
                var users =
                    from tweet in twitter.User
                    where tweet.Type == UserType.Show &&
                          tweet.ScreenName == userName
                    select tweet;

                return users.SingleOrDefault();
            }
        }

        private void UpdateTweets()
        {
            if (!AllTweets.Any())
            {
                AllTweets = GetAllTweets();
                return;
            }
            else
            {
                var newestId = ulong.Parse(AllTweets.First().StatusID);
                var newTweets = GetAllTweets(newestId);
                AllTweets.InsertRange(0,newTweets);
            }
        }

        private List<Status> GetAllTweets(ulong sinceId = 0)
        {
            var allTweets = new List<Status>();

            using (var twitter = new TwitterContext(Auth))
            {
                foreach (var u in Users)
                {
                    ulong localSince = sinceId;
                    int lastCount = 199;
                    var oldestId = ulong.MaxValue;
                    while (lastCount > 1)
                    {
                        IQueryable<Status> statusTweets =
                            twitter.Status.Where(tweet => tweet.Type == StatusType.User
                                                          && tweet.ScreenName == u
                                                          && tweet.IncludeMyRetweet == true
                                                          && tweet.ExcludeReplies == false
                                                          && tweet.Count == 199);

                        if (oldestId != ulong.MaxValue && sinceId == 0)
                            statusTweets = statusTweets.Where(t => t.MaxID == oldestId);

                        if (sinceId != 0)
                            statusTweets = statusTweets.Where(t => t.SinceID == localSince);

                        if (statusTweets != null)
                        {
                            var returned = statusTweets.ToList();

                            if (!returned.Any())
                                break;

                            lastCount = returned.Count();
                            localSince = returned.Max(t => ulong.Parse(t.StatusID));
                            oldestId = returned.Min(t => ulong.Parse(t.StatusID));
                            returned.RemoveAt(returned.Count - 1);
                            allTweets.AddRange(returned);
                        }
                        else
                        {
                            lastCount = 0;
                        }
                    }
                }
            }

            return allTweets.Distinct().Where(t => t.CreatedAt > new DateTime(2015,01,01)).OrderByDescending(s =>s.StatusID).ToList();
        }

    }
}