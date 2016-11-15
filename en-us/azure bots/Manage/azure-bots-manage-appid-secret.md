---
layout: page
title: Managing your MSA App ID and password
permalink: /en-us/azure-bot-service/manage/appid-password/
weight: 13200
parent1: Azure Bot Service
parent2: Manage
---

Azure Bot Service is powered by the Microsoft Bot Framework, which requires an App Id and password to work. If you create a bot via the Azure Bot Service templates, you will be asked to generate the MSA App ID and password during the creation flow, as described in [this document](/en-us/azure-bots/build/first-bot/).

The MSA App ID and password will be saved in the bot application settings with keys:

- MicrosoftAppId
- MicrosoftAppPassword

There are cases when you will need to generate a new password, let's see how you can do that. The easiest way to do it, is to use the Bot Framework Developer Portal.

## Creating a new password and updating your bot configuration
To create a new password, follow these simple steps:

1. Visit the [Bot Framework Developer portal](https://dev.botframework.com/){:target="_blank"}, find your bot in the My bots page, and open it.
2. Click on the "Edit" link in the Details panel
    [![Edit your bot](/en-us/images/azure-bots/msa-password-update-devportal-dashboard.png)](/en-us/images/azure-bots/msa-password-update-devportal-dashboard.png)
3. Click on the "Manage Microsoft App ID and password" button
    [![Manage Microsoft App ID and password](/en-us/images/azure-bots/msa-password-update-devportal-edit.png)](/en-us/images/azure-bots/msa-password-update-devportal-edit.png)
4. If prompted, login with the Microsoft Account you used to create the MSA App ID, then click on "Generate New Password" in the resulting screen
    [![Generate New Password](/en-us/images/azure-bots/msa-password-update-msa-createnew.png)](/en-us/images/azure-bots/msa-password-update-msa-createnew.png)
5. Copy the password in the modal dialog, then close the dialog 
    [![Copy the password](/en-us/images/azure-bots/msa-password-update-msa-pwdcreated.png)](/en-us/images/azure-bots/msa-password-update-msa-pwdcreated.png)

    <div class="docs-text-note"><strong>Note:</strong> This is the only time you'll see the password, copy it, and store it securely.</div>

6. Paste the password in the bot application settings in the Azure Bot Service
    [![Paste the password in the bot application settings in the Azure Bot Service](/en-us/images/azure-bots/msa-password-update-portal.png)](/en-us/images/azure-bots/msa-password-update-portal.png)
7. If you have two passwords already, you will need to delete one of them. Just delete the unused one, and generate a new one following the previous steps
    [![Delete a password](/en-us/images/azure-bots/msa-password-update-msa-pwddelete.png)](/en-us/images/azure-bots/msa-password-update-msa-pwddelete.png)

    <div class="docs-text-note"><strong>Note:</strong> If you delete the password currently configured in your bot, the bot will stop working. Make sure you delete the unused one.</div>

