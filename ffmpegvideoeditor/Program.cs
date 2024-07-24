// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var filevid = "/work/datatemp/SOIN/[CSIP] Course 1/Bài 0 - Tổng quan khóa học.mp4";
var fileblue = "/work/datatemp/SOIN/[CSIP] Course 1/blue.png";
var filered = "/work/datatemp/SOIN/[CSIP] Course 1/red.png";
var r1 = await new CommandExecuter().DrawOverlay(filevid, fileblue, 473, 691, 60 + 55, 60 + 60 + 5);

var r2 = await new CommandExecuter().DrawOverlay(r1.OutputFile, filered, 467, 969, 21.5, 25.5);

await new CommandExecuter().DrawOverlay(r2.OutputFile, filered, 467, 969, 60 + 35, 60 + 39);
//  await new CommandExecuter().SaveFrame(filevid,25*1000,filevid+".red.png");

