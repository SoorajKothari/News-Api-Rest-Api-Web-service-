using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using NewsToday_WebService.Models;
using Newtonsoft.Json;

namespace NewsToday_WebService.Controllers
{
    /*
     * Our controller class
     */
    [RoutePrefix("api/web")]
    public class WebController : ApiController
    {

        //Object to database access layer
        private DataAccessLayer db = new DataAccessLayer();


        //get all news
        [HttpGet]
        [Route("findall")]
        public HttpResponseMessage findall()
        {
            return db.Fetch_All_News();
        }

        //update count of news with id given
        [HttpPut]
        [Route("updatecount/{id}")]
        public void updatecount(int id)
        {
           db.Increment_Popular_News(id);
        }


        //get trending news
        [HttpGet]
        [Route("popular")]
        public HttpResponseMessage popular()
        {
            return db.Get_trending_news();
        }


        //check if user exist if not then add user with given uid which is imei
        [HttpPost]
        [Route("check/{uid}")]
        public void check(string uid)
        {
            db.check(uid);
        }

        [HttpPut]
        [Route("inc/{uid}/{nid}")]
        public void inc(string uid,int nid)
        {
            db.increment_user_count(uid, nid);
        }

        [HttpGet]
        [Route("recomend/{uid}")]
        public HttpResponseMessage recomend(string uid)
        {
            return db.get_recommended_news(uid);

        }

        [HttpGet]
        [Route("recount/{uid}")]
        public HttpResponseMessage recount(string uid)
        {
            return db.recount(uid);
        }


        [HttpPut]
        [Route("bind/{uid}/{mac}")]
        public void bind(string uid,string mac)
        {
            db.bind(uid, mac);
        }

        [HttpGet]
        [Route("getimei/{mac}")]
        public HttpResponseMessage getimei(string mac)
        {
            return db.get_imei_from_mac(mac);
        }

    }
}