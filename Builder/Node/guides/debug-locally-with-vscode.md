---
layout: page
title: Debug Locally with VSCode
permalink: /builder/node/guides/debug-locally-with-vscode/
weight: 612
parent1: Bot Builder for Node.js
parent2: Guides
---

* TOC
{:toc}

## Overview
If you’re building a bot for the Bot Connector Service on a Windows machine you can use the awesome [Bot Framework Emulator](/connector/tools/bot-framework-emulator/) that you can use to debug your bot. Unfortunately, the emulator is currently Windows only so for Mac or Linux users you’ll need to explore other options. One option is to install [VSCode](https://code.visualstudio.com/) and use Bot Builders [TextBot]( /builder/node/bots/TextBot/) class to debug your bot running in a console window. This guide will walk you through doing just that.

## Launch VSCode
For purposes of this walkthrough we’ll use Bot Builders [TodoBot](https://github.com/Microsoft/BotBuilder/tree/master/Node/examples/todoBot) example. After you install VSCode on your machine you should open your bots project using “open folder”.

![Step 1: Launch VSCode](/images/builder-debug-step1.png)

## Launch Bot
The TodoBot illustrates running a bot on multiple platforms which is the key to being able to debug locally. To debug locally you need a version of your bot that can run from a console window using the [TextBot]() class. For the TodoBot we can run it locally by launching the textBot.js class. To properly debug this class using VScode we’ll want to launch node with the –debug-brk flag which cause it to immediately break. So from a console window type “node –debug-brk textBot.js”.

![Step 2: Launch Bot](/images/builder-debug-step2.png)

## Configure VSCode
Before you can debug your now paused bot you’ll need to configure the VSCode node debugger. VSCode knows you project is using node but there are a lot of possible configurations for how you launch node so it makes you go through a one-time setup for each project (folder.)  To setup the debugger select the debug tab on the lower left and press the green run button. This will ask you to pick your debug environment and you can just select “node.js”. The default settings are fine for our purposes so no need to adjust anything.

![Step 3: Configure VSCode](/images/builder-debug-step3.png)

## Attach Debugger
Configuring the debugger resulted in two debug modes being added, Launch & Attach. Since our bot is paused in a console window we want to select the “Attach” mode and press the green run button again.

![Step 4: Attach Debugger](/images/builder-debug-step4.png)

## Debug Bot
VSCode will attach to your bot paused on the first line of code and now you’re ready to set break points and debug your bot! Your bot can be sent communicated with from the console window so switch back to the console window and say “hello”.

![Step 5: Debug Bot](/images/builder-debug-step5.png)

