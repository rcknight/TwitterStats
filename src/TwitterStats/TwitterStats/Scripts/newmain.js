$(document).ready(function () {
    var dailyTweetData = [];
    var dailyCompetitorData = [];
    
    $.get("stats/DailyTweets", function (data) {
        dailyTweetData = data;
        buildMorris(true);
    });
    
    $.get("stats/DailyCompetitors", function (data) {
        dailyCompetitorData = data;
        buildMorris(true);
    });

    $(window).resize(function(e) {
      var morrisResize;
      clearTimeout(morrisResize);
      return morrisResize = setTimeout(function() {
        return buildMorris(true);
      }, 500);
    });
    $(function() {
      return buildMorris();
    });

      buildMorris = function($re) {
          if ($re) {
              $(".graph").html("");
          }
            
            Morris.Area({
                element: "hero-area",
                data: dailyTweetData,
                xkey: "date",
                ykeys: ["tweets", "retweets"],
                labels: ["Tweets", "Retweets"],
                hideHover: "auto",
                lineWidth: 2,
                pointSize: 4,
                lineColors: ["#a0dcee", "#a0e2a0"],
                fillOpacity: 0.5,
                smooth: true
            });

          
          Morris.Bar({
              element: "hero-bar",
              data: dailyCompetitorData,
              xkey: "date",
              ykeys: ["competitors"],
              labels: ["Other Entrants"],
              barRatio: 0.4,
              xLabelAngle: 35,
              hideHover: "auto",
              barColors: ["#00ACED"]
          });
      };
  });