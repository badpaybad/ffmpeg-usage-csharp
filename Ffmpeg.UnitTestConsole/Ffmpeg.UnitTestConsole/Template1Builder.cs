using Ffmpeg.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Ffmpeg.UnitTestConsole
{
    public class Template1Builder : AbstractTemplateBuilder
    {
        string _dir;
        long studentId;
        public string StudentName;
        public Template1Builder()
        {
            _dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OneDayMemoryable/template1");
        }

        protected override void InternalRun()
        {
            var intro = BuildIntro();

            var bg = new FfmpegCommander().WithOutDuration(5)
               .WithInputFile(Path.Combine(_dir, "bg1.jpg"), 1)
               .ToCommand();

            Exec(bg);

            var bgText = new FfmpegCommander().WithOutDuration(bg.Duration)
            .WithInputFile(bg.FileOutput, 5)
            .WithTextOverlay("Hom nay con hoc duoc\nrat nhieu dieu", "white", 40, 200, 50, false, "black")
            .ToCommand();

            Exec(bgText);

            var bgTextBag = new FfmpegCommander().WithOutDuration(bg.Duration)
      .WithInputFile(bgText.FileOutput, 5)
           .WithFileOverlay(Path.Combine(_dir, "bag.gif"), 0, 5, "250:250",  100, 450)
      .ToCommand();

            Exec(bgTextBag);


            var bgTextBagTodo = new FfmpegCommander().WithOutDuration(bg.Duration)
      .WithInputFile(bgTextBag.FileOutput, 5)
           .WithFileOverlay(Path.Combine(_dir, "kcntodo.gif"), 0, 5, "400:500", 700, 80)
      .ToCommand();

            Exec(bgTextBagTodo);

            var bgTextBagTodoText = new FfmpegCommander().WithOutDuration(bg.Duration)
   .WithInputFile(bgTextBagTodo.FileOutput, 5)
        .WithTextOverlay("Xep hinh\n\nVe tranh\n\nKe chuyen", "yellow", 850, 220, 50, false, "black")
   .ToCommand();

            Exec(bgTextBagTodoText);

            var transit = new FfmpegCommander().WithOutDuration(10)
                .WithInputFile(intro.CommadExecuted.FileOutput, 6)
                .WithTransitionNext(bgTextBagTodoText.FileOutput, bgTextBagTodoText.Duration, 2,"slideleft")
                .ToCommand();

            Exec(transit);

            var cmds = GetCommands();
        }


        private FfmpegConvertedResult BuildIntro()
        {
            var bg = new FfmpegCommander().WithOutDuration(5)
                .WithInputFile(Path.Combine(_dir, "bg.jpg"), 1)
                .ToCommand();

            Exec(bg);

            //var faceboy = new FfmpegCommander().WithOutDuration(5)
            //   .WithInputFile(Path.Combine(_dir, "faceboy.png"), 1)
            //   .WithScale(2000, 1000)
            //   .ToCommand();

            //_result.Add(faceboy.Run());

            var bgText = new FfmpegCommander().WithOutDuration(bg.Duration)
                .WithInputFile(bg.FileOutput, 5)
                .WithTextOverlay("         Xin chao con la Bao Anh\n\nMoi ngay den lop duoc gap\n     co va cac ban that tuyet", "white", 40, 200, 50, false, "black")
                .ToCommand();

            Exec(bgText);

            var bgTextCar = new FfmpegCommander().WithOutDuration(bg.Duration)
                .WithInputFile(bgText.FileOutput, 5)
                .WithFileOverlay(Path.Combine(_dir, "car.gif"), 0, 5, "320:300", 50, 450)
                .ToCommand();

            Exec(bgTextCar);

            var bgTextCarFace = new FfmpegCommander().WithOutDuration(bg.Duration)
               .WithInputFile(bgTextCar.FileOutput, 5)
               .WithFileOverlay(Path.Combine(_dir, "studentface.png"), 0, 5, "340:320", 725, 200)
               .ToCommand();

            Exec(bgTextCarFace);

            var bgTextCarFaceKhung = new FfmpegCommander().WithOutDuration(bg.Duration)
              .WithInputFile(bgTextCarFace.FileOutput, 5)
              .WithFileOverlay(Path.Combine(_dir, "kvintro.gif"), 0, 5, "400:350", 695, 178)
              .ToCommand();

            return Exec(bgTextCarFaceKhung);
            //var bgTextCarFaceKhungFace = new FfmpegCommander().WithOutDuration(5)
            //   .WithInputFile(bgTextCarFaceKhung.FileOutput, 5)
            //   .WithFileOverlay(Path.Combine(_dir, "studentface.png"), 0, 5, "140:130", 720, 410)
            //   .ToCommand();

            //_result.Add(bgTextCarFaceKhungFace.Run());


            //var bgTextCarFaceKhungFaceBoy = new FfmpegCommander().WithOutDuration(5)
            //   .WithInputFile(bgTextCarFaceKhungFace.FileOutput, 5)
            //   .WithFileOverlay(Path.Combine(_dir, "faceboy.png"), 0, 5, "560:280", 510, 380)
            //   .ToCommand();

            //_result.Add(bgTextCarFaceKhungFaceBoy.Run());

            //var addSound = new FfmpegCommander().WithOutDuration(bgTextCarFaceKhung.Duration)
            //.WithInputFile(bgTextCarFaceKhung.FileOutput, bgTextCarFaceKhung.Duration)
            //.WithAudio(Path.Combine(_dir, "a.mp3")).ToCommand();

            //_result.Add(addSound.Run());

            //    var addLogo = new FfmpegCommander().WithOutDuration(addSound.Duration)
            //.WithInputFile(addSound.FileOutput, addSound.Duration)
            //.WithFileOverlay(Path.Combine(_dir, "kologo.png"), 0, addSound.Duration, "150:82", 20, 20)
            //.ToCommand();

            //    _result.Add(addLogo.Run());

        }

        static ConcurrentDictionary<string, Func<WebRequest, WebResponse>> _map = new ConcurrentDictionary<string, Func<WebRequest, WebResponse>>();
        public void Register(string type, Func<WebRequest, WebResponse> fuc)
        {
            _map.GetOrAdd(type, fuc);
        }

        public void RegisterAtBootup()
        {
            Register("xxx", DoXxx);
            Register("yyy", DoXxx);
        }

        public WebResponse DoXxx(WebRequest req)
        {
            //you businee here

            return null;
        }

        public WebResponse Handle(WebRequest request)
        {
            var type = "";
            //get type from request
            if (_map.TryGetValue(type, out Func<WebRequest, WebResponse> hanlde))
            {
                var response = hanlde(request);
                return response;
            }

            return null;
        }
    }


}
