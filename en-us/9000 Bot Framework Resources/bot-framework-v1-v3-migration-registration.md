---
layout: page
title: Upgrade your bot registration to V3
permalink: /en-us/support/upgrade-to-v3/
weight: 9150
parent1: Bot Framework Resources
---


We've introduced a new iteration of our API (V3).  In this API, there are a number of small changes designed to make the API more adaptable to future requirements.

Upgrading your bot to our new API (V3) is very easy, just follow these simple steps:

* TOC
{:toc}

## Initial recommendations

1. Read through this guide completely before starting, so you understand the whole process.
2. If you have current users using your bot, and don't want disruptions, we recommend migrating the code in a separate endpoint, and switch when everything works.

## 1. Get your App ID and password from the Developer Portal

Visit the [Bot Framework Developer Portal](https://dev.botframework.com/), look for your bot, visit its dashboard, and click on the Edit button.

Find the Configuration info panel and look at the App ID field. Follow these instructions depending on your case.

### Case 1: There is an App ID already

1. Click on "Manage App ID and password" 
    ![](/en-us/images/migration/manage-app-id.png)
2. Generate a new Password 
    ![](/en-us/images/migration/generate-new-password.png)
3. Copy and save the new password along with the MSA App Id, you will need them later on. 
    ![](/en-us/images/migration/new-password-generated.png)

### Case 2: There is no App ID

1. Click on "Generate App ID and password".
    ![](/en-us/images/migration/generate-appid-and-password.png)
    <div class="docs-text-note"><strong>Note:</strong> Do not check the Version 3.0 radio button until you have migrated your bot code.</div>
2. Click on "Generate a password to continue".
    ![](/en-us/images/migration/generate-a-password-to-continue.png)
3. Copy and save the new password along with the MSA App Id, you will need them later on. 
    ![](/en-us/images/migration/new-password-generated.png)
4. Click on "Finish and go back to the Bot Framework portal".
    ![](/en-us/images/migration/finish-and-go-back-to-bot-framework.png)
5. Once back in the Bot Framework portal, scroll all the way down and save your changes.
    ![](/en-us/images/migration/save-changes.png)

## 2. Update your bot code to Version 3.0

This is the most involved part of the process, [follow this detailed guide](/en-us/support/upgrade-code-to-v3/) to get started. You will need the  App ID you got in the previous step.

Once you're done, come back to this page and continue from here.

## 3. Update your bot registration in the Bot Framework Developer Portal

Once you have deployed your new V3 bot, you're ready to test. 

1. Visit the [Bot Framework Developer Portal](https://dev.botframework.com/), look for your bot, visit its dashboard, and click on the Edit button. Find the Configuration section.
2. Paste your new endpoint in the "Messaging endpoint" field of the "Version 3.0" section.
    ![](/en-us/images/migration/paste-new-v3-enpoint-url.png)
3. Click the Version 3.0 endpoint radio button, and save your changes.
    ![](/en-us/images/migration/switch-to-v3-endpoint.png)
    <div class="docs-text-note"><strong>Note:</strong> When you do this, your bot will switch to the new endpoint. If anything goes wrong, you can revert it back to Version 1.0, and iterate until Version 3.0 works properly.</div>
4. Once Version 3.0 works as expected, you're done.


