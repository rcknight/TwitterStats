using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TwitterStats.Models;

namespace TwitterStats.Controllers
{
    public class StatsController : Controller
    {
        //
        // GET: /Stats/
        public ActionResult Index()
        {
            if (MvcApplication.Stats == null)
            {
                MvcApplication.Stats = new StatsViewModel();
            }
            else
            {
                MvcApplication.Stats.Update();
            }

            return View(MvcApplication.Stats);
        }

        public JsonResult DailyTweets()
        {
            if (MvcApplication.Stats == null)
            {
                MvcApplication.Stats = new StatsViewModel();
            }
            else
            {
                MvcApplication.Stats.Update();
            }

            return new JsonResult() {Data = MvcApplication.Stats.DailyTweets, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

        public JsonResult DailyCompetitors()
        {
            if (MvcApplication.Stats == null)
            {
                MvcApplication.Stats = new StatsViewModel();
            }
            else
            {
                MvcApplication.Stats.Update();
            }

            return new JsonResult() { Data = MvcApplication.Stats.DailyCompetitors, JsonRequestBehavior = JsonRequestBehavior.AllowGet};
        }

    }
}
