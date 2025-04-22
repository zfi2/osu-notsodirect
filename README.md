# osu!notsodirect

> no supporter? no problem!

osu!notsodirect is a hacky, but functional local replacement for osu!direct.
it downloads beatmaps from public mirrors and launches them through osu!, effectively auto-extracting them — just like the real osu!direct... but for free :3


https://github.com/user-attachments/assets/df54407d-70d5-4ba2-bc3c-2a432a633017



## 💡 how it works

- downloads `.osz` files from public mirrors (like `catboy.best` or `nerinyan.moe`)
- auto-imports them into osu!
- requires *zero* changes to the game itself (no ban risk, no funny business)
- it's also like... *blazingly fast*

## 🧱 setup and prerequisites

1. **important notes:**
   - ***!!! THE GAME MUST RUN IN BORDERLESS FULLSCREEN/BORDERLESS WINDOW !!!*** if it's not, it will minimize your game, and show the menu on your desktop.
   - make sure you have **.NET 9.0 Windows Desktop Runtime** installed. [download link](https://builds.dotnet.microsoft.com/dotnet/windowsdesktop/9.0.4/windowsdesktop-runtime-9.0.4-win-x64.exe).
   - if you wanna compile it yourself, make sure you have the **.NET 9.0 Windows SDK** installed. [download link](https://builds.dotnet.microsoft.com/dotnet/sdk/9.0.203/dotnet-sdk-9.0.203-win-x64.exe)

2. **where to download and keybinds**
   - you can download and run the program from the releases page on github, or compile it yourself.
   - when you press the **HOME** key on your keyboard you can **show/hide the overlay**, and **the F12 key shows the debug console**.

3. **summary**
   - after doing these things, you should be set and ready to go! now just search for a map, click on the download button, and the map should auto-import itself to osu!

## possible issues
- borderless fullscreen introduces input delay, unfortunately, there's not much I can do to fix this, it's a stupid Windows issue. [Forced DWM buffering](https://osu.ppy.sh/community/forums/topics/774452?n=3)
- however, you can minimize the input lag by disabling Fullscreen Optimizations for the osu!.exe file, like this:
![XsFthNF3Q0](https://github.com/user-attachments/assets/66c363eb-688f-49ea-8695-ca28a2f1545e)


## wanna donate?

if you like what i made, consider supporting me — any donation helps and means a lot!
- bitcoin: bc1qkhtl7m6vggyhdynw34nda28eytlv9jfmqdkpws
- ethereum: 0xd1350B30D035D82D77Ac5619be0e93F66848EF7B
- litecoin: LU6abVwrDRKNc4aLi2gPUeKF6hTYiwVBfG
- monero: 4AJvEWRpUAjEyA8ML9ZkEHeQYFscioRXS8EVk5HMKH63dJj3QpfpQr5YyTwNHvDfsXVpns486TBAvMsoiTy1BGBJER1KCvp

- other ways? just hit me up on discord — it’s on my site: [lain.ovh](https//lain.ovh/)
thanks a ton ❤️
