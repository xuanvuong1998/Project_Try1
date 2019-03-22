﻿using Project_Try1.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI;

namespace Project_Try1.Controllers {
    public class CreateQuizController : Controller {
        // GET: CreateQuiz
        [Authorize]
        
        public ActionResult Index() {
            
            return View("CreateNewQuiz");
        }

        [HttpPost]
        public ActionResult PostImage(HttpPostedFileBase file, FormCollection frm)
        {
            string title = frm["TxtTitle"];
            if (file != null && file.ContentLength > 0)
                try
                {
                    string path = Path.Combine(Server.MapPath("~/resources/images/QuizImages"),
                        Path.GetFileName(file.FileName));
                    file.SaveAs(path);

                    // WebImage belong to WebHelper class which supports the crop, flip, watermark operation etc.
                    WebImage img = new WebImage(file.InputStream);
                    if (img.Width > 1200)
                        img.Resize(1200, 600);
                    img.Save(path);


                    ViewBag.Message = "File uploaded successfully";
                    Session["Title"] = title;
                    Session["Image"] = file.FileName;

                    return View("AddQuestion");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
          
            return View("CreateNewQuiz");
        }
        [Authorize]
        public ActionResult QuestionDetails() {
            return View("QuestionDetails");
        }
  
        [Authorize]
        public ActionResult AddQuestionToQuiz(FormCollection frm) {
            dynamic list = Session["QuestionList"];

            if (list == null) {
                list = new List<Question>();
            }

            string c1 = frm["TxtC1"];
            string c2 = frm["TxtC2"];
            string c3 = frm["TxtC3"];
            string c4 = frm["TxtC4"];
            string ans = frm["TxtAns"];
            string image = frm["TxtImage"];

            Question q = new Question {
                Content = frm["TxtContent"],
                AnsA = c1,
                AnsB = c2,
                AnsC = c3,
                AnsD = c4,
                Time = int.Parse(frm["TxtTime"]),
                Answer = ans,
                Image = image
            };

            list.Add(q);
            Session["QuestionList"] = list;

            return View("AddQuestion");
        }

        [Authorize]
        public RedirectToRouteResult CancelCreatingQuiz() {
            Session["QuestionList"] = null;
            Session["Title"] = null;
            Session["Image"] = null;


            return RedirectToAction("Index", "MyQuiz");
        }

        [Authorize]
        public ActionResult CancelCreatingQuestion() {

            return View("AddQuestion");
        }

        [Authorize]
        public RedirectToRouteResult SaveQuiz() {
            var queList = (List<Question>)Session["Questionlist"];

            QuestionDM queDM = new QuestionDM();
            QuizBank quizBank = new QuizBank();

            Quiz q = new Quiz {
                ID = quizBank.GetMaxID() + 1,
                Title = (string)Session["Title"],
                Image = (string)Session["Image"],
                Creator = (string)Session["Creator"],
                QuestionList = new List<Question>()
            };


            if (string.IsNullOrEmpty(q.Image)) {
                q.Image = "default.png";
            }
            int maxID = queDM.GetMaxID();

            if (queList != null) {
                foreach (var item in queList) {
                    item.ID = ++maxID;
                    item.QuizID = q.ID;
                    q.QuestionList.Add(item);
                }

            }
            quizBank.AddNewQuiz(q);

            Session["Title"] = null;
            Session["Image"] = null;
            Session["QuestionList"] = null;

            return RedirectToAction("index", "MyQuiz");

        }

    }
}