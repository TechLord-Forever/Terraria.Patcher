using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dnlib.DotNet.Emit;
using dnpatch;

namespace Terraria_Patcher
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
             * Remove Steam Check from Terraria
             */
            Console.WriteLine("Removing Steam...");
            Patcher p = new Patcher("Terraria.exe");
            Target target = new Target()
            {
                Namespace = "Terraria.Social",
                Class = "SocialAPI",
                Method = "LoadSteam"
            };
            p.WriteEmptyBody(target);

            /*
             * Get the start index and the last index of the Collectors Edition Check and remove them.
             * By not using predefined indexes we can guarantee that this will work in future even if they make changes.
             */
            Console.WriteLine("Unlocking Collectors Edition...");
            target = new Target()
            {
                Namespace = "Terraria",
                Class = "Main",
                Method = "CheckBunny",
            };
            int startIndex = p.FindInstruction(target, Instruction.Create(OpCodes.Ldloc_0), 2);
            int endIndex = p.FindInstruction(target, Instruction.Create(OpCodes.Ldc_I4_1)) - 1;
            target.Indices = Enumerable.Range(startIndex, endIndex - startIndex + 1).ToArray();
            p.RemoveInstruction(target);
            Console.WriteLine("Unlocked!");

            /*
             * Patch operands which contain the random titles
             */
            Console.WriteLine("Adding message to title...");
            target = new Target()
            {
                Namespace = "Terraria",
                Class = "Lang",
                Method = "title"
            };
            Target[] titleTargets = p.FindInstructionsByOpcode(target, new[] { OpCodes.Ldstr });
            foreach (var index in titleTargets[0].Indices)
            {
                Target t = target;
                t.Index = index;
                string title = p.GetOperand(t);
                if (title.Contains("Terraria: "))
                {
                    p.PatchOperand(t, title + " - Support The Developers!");
                }
            }
            Console.WriteLine("Done");
            p.Save(true);
            Console.WriteLine("Saved!");
            System.Threading.Thread.Sleep(500);
        }
    }
}
