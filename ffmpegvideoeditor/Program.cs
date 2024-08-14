// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

// if(args.Length<1) {

// Console.WriteLine("ffmpegvideoeditor getframe {atmilisecond} {filevideo} \r\neg: ffmpegvideoeditor getframe 1005 \"/work/dunp.mp4\"");

// Console.WriteLine("ffmpegvideoeditor apply \"fullpathfiledata\" eg: ffmpegvideoeditor apply \"/work/dunp.txt\"");
//     return;
// }

var filevid = "/work/datatemp/SOIN/[CSIP] Course 1/Bài 0 - Tổng quan khóa học.mp4";
var fileblue = "/work/datatemp/SOIN/[CSIP] Course 1/blue.png";
var filered = "/work/datatemp/SOIN/[CSIP] Course 1/red.png";
var filebluesmall = "/work/datatemp/SOIN/[CSIP] Course 1/bluesmall.png";
// var r1 = await new CommandExecuter().DrawOverlay(filevid, fileblue, 473, 691, 60 + 55, 60 + 60 + 5);

// var r2 = await new CommandExecuter().DrawOverlay(r1.OutputFile, filered, 467, 969, 21.5, 25.5);

// await new CommandExecuter().DrawOverlay(r2.OutputFile, filered, 467, 969, 60 + 35, 60 + 39);


//  await new CommandExecuter().SaveFrame(filevid,25*1000,filevid+".red.png");

// /work/datatemp/SOIN/[CSIP] Course 1/Bài 0 - Tổng quan khóa học.mp4 , 21.5, 25.5, red,467,969 
// /work/datatemp/SOIN/[CSIP] Course 1/Bài 0 - Tổng quan khóa học.mp4 , 95, 69, red, 467,969 
// /work/datatemp/SOIN/[CSIP] Course 1/Bài 0 - Tổng quan khóa học.mp4 , 115, 125, blue, 473,691 

// var finall = await new SoinApplyMass().Do(filevid, new System.Collections.Generic.List<SoinOverlay>{

//     new SoinOverlay{
//         FromSeconds=21.5,
//         ToSeconds= 25.5,
//         ImageOverlayFilePath= filered,
//         X=467,
//         Y=969
//     },
//     new SoinOverlay{
//         FromSeconds= 60 + 35,
//         ToSeconds= 60 + 39,
//         ImageOverlayFilePath= filered,
//         X=467,
//         Y=969
//     },
//     new SoinOverlay{
//         FromSeconds=60+55,
//         ToSeconds=60+60+5,
//         ImageOverlayFilePath= fileblue,
//         X=473,
//         Y=691
//     },
// });
var xxx = new SoinApplyMass();
var listall = await xxx.Parse("/work/ffmpeg-usage-csharp/ffmpegvideoeditor/videoedit.txt");

foreach (var v in listall)
{

    var finall = await xxx.Do(v.OriginalVideoFilePath, v.Overlays);
    Console.WriteLine("Finnal: " + finall);

}


