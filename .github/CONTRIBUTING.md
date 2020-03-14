<!-- 
CONTRIBUTING.md
OhScrap
created: 2020 03 13
updated: 2020 03 14
-->

[MOD:license]:      https://github.com/zer0Kerbal/OhScrap/blob/master/LICENSE
[MOD:contributing]: https://github.com/zer0Kerbal/OhScrap/blob/master/.github/CONTRIBUTING.md
[MOD:issues]:       https://github.com/zer0Kerbal/OhScrap/issues
[MOD:wiki]:         https://github.com/zer0Kerbal/OhScrap/
[MOD:known]:        https://github.com/zer0Kerbal/OhScrap/wiki/Known-Issues
[MOD:forum]:        https://forum.kerbalspaceprogram.com/index.php?/topic/178641-*
[SHIELD:mod]: https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/zer0Kerbal/OhScrap/master/json/mod.json
[SHIELD:ksp]: https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/zer0Kerbal/OhScrap/master/json/ksp.json
[SHIELD:license]: https://img.shields.io/endpoint?url=https://raw.githubusercontent.com/zer0Kerbal/OhScrap/master/json/license.json

# Before you Contribute (thank you!) Please Read:
> [LICENSE][MOD:license] ![SHIELD:license] ![LOGO:mit]  
> [CONTRIBUTING][MOD:contributing]

### All submissions become subject to this repository's [LICENSE][MOD:license] ![SHIELD:license] ![LOGO:mit]  
##### Under GitHub Terms of Service (ToS), all pull requests are licensed under the target repository license, unless a different agreement is previously reached.
### All push requests are licensed under: ![SHIELD:license] ![LOGO:mit]  

# Submitting changes
Please send a GitHub Pull Request to OhScrap with a clear list of what you've done (read more about pull requests). When you send a pull request, we will love you forever if you include RSpec examples. We can always use more test coverage. Please follow our coding conventions (below) and make sure all of your commits are atomic (one feature per commit).

Always write a clear log message for your commits. One-line messages are fine for small changes, but bigger changes should look like this:

$ git commit -m "A brief summary of the commit
> 
> A paragraph describing what changed and its impact."

Coding conventions

Start reading our code and you'll get the hang of it. We optimize for readability:

    - We indent using tabs (or if must, 4 spaces)
    - We ALWAYS put spaces after list items and method parameters ([1, 2, 3], not [1,2,3]), around operators (x += 1, not x+=1), and around hash arrows.
    - This is open source software. Consider the people who will read your code, and make it look nice for them. It's sort of like driving a car: Perhaps you love doing donuts when you're alone, but with passengers the goal is to make the ride as smooth as possible.
    -So that we can consistently serve images from the CDN, always use image_path or image_tag when referring to images. Never prepend "/images/" when using image_path or image_tag.
    - Also for the CDN, always use cwd-relative paths rather than root-relative paths in image URLs in any CSS. So instead of url('/images/blah.gif'), use url('../images/blah.gif').
    - button textures, sounds, and settings save files should all be under /KSP/GameData/%MOD%/Plugins/PluginData/ 

***

*click the red box because it is also a link*
[![How to get support][image:get-support]][thread:getsupport]

# Read this page before reporting a bug.
### *If you ignore these directions, your report may be ignored.*

### Before you report

**Has it already been reported?**  
Check the [issue tracker][MOD:issues] and the [Known issues][MOD:known] to see if the problem has already been reported. If so, see if you can contribute additional information, without adding a new issue. 
If you're not sure if your issue is related, comment on the existing report first.

**Is it intended behavior? / Are you doing it right?**  
Make sure you're encountering a bug and not just an intended aspect of the mod.

**Is it actually a ![SHIELD:mod] problem?**  
See if the problem occurs if you uninstall this mod, and also see if it occurs when this is the *only* mod installed. 

**Are you up-to-date?**  
Only the latest version of ![SHIELD:mod] and the latest version of ![SHIELD:ksp] are supported. Make sure both are completely up-to-date before filing a report.

### Filing a report
Bug reports and feature requests should be filled on the [issue tracker][MOD:issues] here on Github. Please read the following guidelines for filing reports; it's very difficult for me to help otherwise.

* **Be specific.** The title and description of your report should describe exactly what isn't working. Provide reproduction steps if you can, and explain in detail the symptoms of the issue and what causes it.
* **Provide your output logs.** 
  - First file can be found at `KSP/KSP_Data/output_log.txt`. This contains debug information about your last KSP session. Without this, I cannot diagnose most issues. 
  - Second file is found at `KSP/KSP.log`
  - Include the contents of `KSP/Logs/` 
  - If your report has precise reproduction steps and the cause is obvious, the log is optional. When in doubt, please include it.
* **List other mods.** While mod compatibility issues are rare, you should list all the other mods you have installed.  
>
> * CKAN users now can goto `File` then `Save installed mod list...`
> * Windows users can open a command prompt in `KSP/GamaData` and type `dir /b /w > gamedata.txt` and include the newly created `gamedata.txt` file. *Don't forget to delete the `gamedata.txt` after you send it*
* **Screenshots.Screenshots.Screenshots**
  - If possible, include one or more screenshots
  - a picture is worth a thousand words.
  - pictures draw attention
  - some things can't be explained with just words 
* **System specifications.** Include your hardware specifications (CPU, GPU, RAM) and your operating system. Also include whether you're using Steam or the manual download of KSP and the folder where KSP is installed.
* **One issue per report.** If you have multiple issues, submit multiple reports. Don't lump everything together; it becomes difficult to track disparate issues that way.
* **.craft and saves**
  - sometimes including a .craft file or save file is beneficial
If you don't have enough information to file a bug report, you may ask questions on the [forum thread][MOD:forum]. **Do not send private messages about bugs unless you believe the bug is an exploitable security issue.**

<!-- 
this file is licensed:
GPLv2 2020
zer0Kerbal
-->

[LOGO:mit]:    https://i.postimg.cc/bvjfsMP5/MIT-17x17.png
[LOGO:gplv3]: https://i.postimg.cc/90kCDs7K/gplv3-48x17.png

[image:get-support]: https://i.postimg.cc/vHP6zmrw/image.png

[thread:getsupport]: https://forum.kerbalspaceprogram.com/index.php?/topic/83212-*

[LINK:zer0Kerbal]: https://forum.kerbalspaceprogram.com/index.php?/profile/190933-zer0kerbal/