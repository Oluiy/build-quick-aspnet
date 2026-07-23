`# Regenerate demo.gif:
#   1. dotnet build (from the repo root, so bin/Debug/net8.0/create-aspnet-app.dll exists)
#   2. brew install vhs   (https://github.com/charmbracelet/vhs)
#   3. vhs docs/assets/demo.tape   (run from the repo root)

Output "docs/assets/demo.gif"

Set Shell "zsh"
Set FontSize 15
Set Width 1300
Set Height 1050
Set Theme "Dracula"
Set TypingSpeed 55ms
Set Padding 15

Hide
Type "rm -rf /tmp/bqp-demo && mkdir -p /tmp/bqp-demo && BQP_DLL=$PWD/bin/Debug/net8.0/create-aspnet-app.dll && BuildQuickPkg() { dotnet $BQP_DLL } && cd /tmp/bqp-demo && clear"
Enter
Show

Type "BuildQuickPkg"
Enter
Sleep 1500ms

Type "TaskFlowApi"
Sleep 500ms
Enter
Sleep 700ms

Enter
Sleep 700ms

Enter
Sleep 700ms

Enter
Sleep 700ms

Enter
Sleep 700ms

Enter
Sleep 700ms

Enter
Sleep 700ms

Down
Sleep 400ms
Enter
Sleep 700ms

Type "y"
Sleep 400ms
Enter
Sleep 700ms

Type "y"
Sleep 400ms
Enter
Sleep 6000ms

Type "find TaskFlowApi -type f | sort"
Enter
Sleep 3500ms
