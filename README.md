# KorraAI

![](../../blob/master/Images/Joi1.png?raw=true)

KorraAI is a framework that allows you to create your own bot, also called Embodied Conversational Agent ([ECA](https://en.wikipedia.org/wiki/Embodied_agent)). You can learn more about the design principles in [Architecture](../../wiki/Architecture) and [Philosophy](../../wiki/Philosophy) and the technical aspects in [TechnicalSpec](../../wiki/TechnicalSpec).

[Demo Windows x64](../../wiki/Demo)

Advantages of using KorraAI:

* Has a high quality 3D model, voice synthesis, lips sync, bot movement animation, gaze and blinks, voice annotation (SSML)
* It is plugin based, so some of the technical aspects are hidden for the sake of simplicity, so you can focus on modelling
* Being proactive by design, the bot will continuously initiate interaction in a distribution controlled manner
* Comes with [Probabilistic Programming library](https://github.com/joashc/csharp-probability-monad) that allows for encoding of distributions and Bayesian networks that represent concepts from real-life
* Behavior change can be encoded based on elapsed time or external factors, user responses, etc.
* The result is a Unity 3D application that can be executed on many platforms: Windows, Android ... 

In order to start coding for KorraAI you need to go through the following pages:

*  [Coding and compiling your bot](../../wiki/Coding-your-own-bot)
*  [Architecture](../../wiki/Architecture)
*  [Tutorial](../../wiki/Tutorial)
*  [KorraAIModel class](../../wiki/KorraAIModel-class)
*  [Add a concept](../../wiki/Add-a-concept)
*  [Probabilistic Examples](../../wiki/Probabilistic-Examples)

And also:
*  [Building a response](../../wiki/Building-a-response)
*  [Distribution over the items inside a category](../../wiki/Distribution-inside-a-category)
*  [Adjust when a category runs out of items](../../wiki/Planning-and-running-out-of-items)

**Application:**

- A character in a video game
- Teaching agent
- Sales agent
- Entertainer
- Companion
- Psychological experiments
- Coaching

**License**:

The code in this repository is GPL-ed. The ECA models are free to re-use. KorraAI uses several commercial components that need a licesense. If it is not for simple try and experiment you must purchase the following components:

https://assetstore.unity.com/packages/tools/animation/salsa-lipsync-suite-148442

https://assetstore.unity.com/packages/tools/audio/rt-voice-pro-41068

https://assetstore.unity.com/packages/tools/video/youtube-video-player-youtube-api-29704


