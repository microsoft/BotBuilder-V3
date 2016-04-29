---
layout: page
title: Bot review guidelines
permalink: /directory/review-guidelines/
weight: 720
parent1: Bot Directory
---

<style>
      ol.list-counter {
        padding:1ex 0;
        counter-reset: category;
        list-style-position: inside;
      }
      ol.list-counter > li {
        font-size: larger;
        margin-top: 1rem;
        margin-bottom: 1rem;
        counter-increment: category;
      }
      
      ol.list-counter ol {
        padding:1ex 0;
        list-style-type: decimal;
        list-style-position: inside;
        margin-left: 2em;
        margin-bottom: 1em;        
      }
      ol.list-counter ol li {
        position: relative;        
        display: block;
        counter-increment: item;
        padding-left: 1ex;
        padding-bottom: 1ex;
      }
      
      ol.list-counter ol li:firt-child {
        position: relative;        
        display: block;
        counter-reset: item;
      }
      
      ol.list-counter  ol  li:before {
        position: absolute;        
        content: counter(category)'.'counter(item);
        left: -2em;
        text-align: right;        
      }

    </style>

We welcome you and thank you for investing your talents and time in building Bots for Skype. We have published our Bot Review Guidelines to give you an understanding of the approval process that is involved after you submit your bot and the criteria we use to evaluate Bots in Skype.


<section>
    <ol class="list-counter">
      <li>Functionality</li>
        <ol>
          <li>Bot thats does not do something meaningful that adds value to the user (e.g., respond to basic commands, have at least some level of communication etc.) will be rejected.</li>
          <li>Bot that does not operate as described in its bot description, profile, terms and use and privacy policy will be rejected</li>
          <p>You must notify Microsoft in advance if you make any material change to your bot. Microsoft has the right, in its sole discretion, to remove your bot from the Bot Directory without notice.</p>        
        </ol>
      <li>Terms of Serivces</li>
        <ol>
            <li>Bot that does not provide Terms of Service and Privacy Policy links will be rejected</li>
            <li>Bot that does not operate in accordance with the requirements set forth in the Microsoft <a href="//aka.ms/bf-terms">Online Service Agreement</a> and Microsoft Bot Framework <a href="//aka.ms/bf-conduct">Code of Conduct</a> will be rejected.</li>
            <li>Bot with inactive publisher email account (we will use that for all bot-related communication with you) will be rejected. </li>
        </ol> 
      <li>Metadata (name, description, icons, tags, etc)</li>
        <ol>
            <li>Bot metadata that doesn't reflect its functionality will be rejected.</li>
            <li>Bot metadata that is offensive or explicit will be rejected.</li>
            <li>Bot metadata that includes third party trademarks, service marks or logos will be rejected.</li>
            <li>Bot metadata that impersonates or implies endorsement by a third party will be rejected.</li>
            <li>Bot metadata that uses names unrelated to the bot will be rejected.</li>
            <li>Bot metadata that uses Microsoft logos, trademarks or service marks unless you have permission from Microsoft will be rejected.</li>
            <li>Bot metadata that is too long or verbose (the description should be 8-10 words)  will be rejected.</li>
        </ol>
        <p>Changes made to your bot's metadata may require your bot to be re-reviewed to ensure that it continues to meet the requirements stated here.</p>
                <p>
          Although Microsoft will review your bot to confirm it meets certain minimum requirements prior to publication on the Bot Directory, you are solely responsible for: 
          (1) your bot; (2) its content and actions; (3) compliance with all applicable laws; (4) compliance with any third party terms and conditions; 
          and (5) compliance with the Microsoft Online Services Agreement, Privacy Statement and Microsoft Bot Framework Code of Conduct.  
          Microsoft’s review and publication of your bot to the Bot Directory is not an endorsement of your bot.
        </p>

    </ol>
  </section>