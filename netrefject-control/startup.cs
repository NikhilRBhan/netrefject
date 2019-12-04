﻿using System;
using System.IO;
using System.Diagnostics;
using netrefject;
using System.Linq;

using Mono.Cecil;
using Mono.Cecil.Cil;

public static class Startup
{
    public static void Main(string[] args)
    {
        /*
        if (args.Length != 1)
        {
            Worker.syntax();
            return;
        }

        string filename = args[0];

        if (!filename.EndsWith("dll"))
        {
            Worker.syntax();
            return;
        }*/

        /*
        Proper stuff commented while I fight with the reference shyze
        */
        string filename = "../testlibrary/bin/Debug/netcoreapp2.0/testlibrary.dll";
        Worker w = new Worker();
        w.HandleInjectionFlow(filename);
        return;

        Console.WriteLine("START Running unmodified DLL");
        ProcessStartInfo psi = new ProcessStartInfo("dotnet");
        psi.WorkingDirectory = "../testconsole";
        psi.Arguments = "run testconsole.csprj";
        var p1 = Process.Start(psi);
        p1.WaitForExit();
        Console.WriteLine("END Running unmodified DLL");

        AssemblyDefinition targetAsm = AssemblyDefinition.ReadAssembly(filename);
        TypeDefinition targetType = targetAsm.MainModule.Types.FirstOrDefault(x => x.Name == "Testclass");
        MethodDefinition m1 = targetType.Methods.FirstOrDefault(x => x.Name == "TestMethod");

        Console.WriteLine("Modifying TestClass");
        ILProcessor ilp = m1.Body.GetILProcessor();
        var ldstr = ilp.Create (OpCodes.Ldstr, "INJECTED EVIL");
        var call = ilp.Create (OpCodes.Call,m1.Module.Import (typeof (Console).GetMethod ("WriteLine", new [] { typeof (string) })));
        ilp.InsertBefore (m1.Body.Instructions [0], ldstr);
        ilp.InsertAfter (m1.Body.Instructions [0], call);


        targetAsm.Write("TESTASM.dll");



        Console.WriteLine("Overwriting old version");        
        File.Copy("TESTASM.dll","../testconsole/bin/Debug/netcoreapp2.0/testlibrary.dll",true);
        
        Console.WriteLine("START Running modified DLL");
        psi = new ProcessStartInfo("dotnet");
        psi.WorkingDirectory = "../testconsole";
        psi.Arguments = "run --no-build testconsole.csprj";
        var p2 = Process.Start(psi);
        p2.WaitForExit();
        Console.WriteLine("END Running modified DLL");
    }
}