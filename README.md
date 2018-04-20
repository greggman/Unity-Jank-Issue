# Unity Jank Issue

This is a small project to show an jank issue I'm seeing in Unity.

To repo, get a Mac, plug it into a TV via HDMI. Run this app on the HDMI TV

To run export the project to an app then from the command line run it like this

    open "path-to-app.app" --args --showfps

This will run it outside the editor so as to avoid all the overhead of the edtior.
You should notice the object swinging left and right moves smoothly. No frames
are missed.

Quit (Cmd-Q) and run it again like this

    open "path-to-app.app" --args --showfps --shoot

Now you should see the object swinging left to right is no longer moving smoothly.

Note the FPS meter shows a solid/good framerate. The blue is the amount of time to
process a frame. Blue going all the way from the top of the graph to the bottom would
mean a frame is taking 1/60th of a second. Going even further down would mean it's
taking even longer but as you can see it's taking almost no time at all.

The green line represents the framerate which you can also see is a relatively
smooth 60fps.

I tried capturing a video of the issue using screen capture in QuickTime but interestingly
even though I saw the jank while recording when I playback the recording itself it's
smooth.

I recorded with my iPhone set to record at 60fps. Unfortunately youtube converted to 30fps on
upload :(  Still, if you pay attention you can see that [this video](https://www.youtube.com/watch?v=CjS3s8PPFvA)
is *mostly* smooth. It's only showing 30fps and there are 3 or 4 janks early on
(which weren't there in reality, it's a sync issue between the phone and the TV),
but basically the motion of the thing swinging left and right is smooth though most of the video.

Where as [this video](https://www.youtube.com/watch?v=4ycytP9Qgvk) with the ships shooting
it should be very clear is full of jank. The motion of the object swinging back and forth is
not smooth at all and yet if you look at the FPS meter in the top left corner you can see
processing time (blue) is almost nothing. It would fill the graph all the way to the green
if it was taking 1/60 of a second to compute a frame. The green is the framerate and it's a
pretty consistent 60fps.

**HOW DO FIX THIS JANK ISSUE?**

Using a standard monitor like the monitor built into a Mac laptop or an iMac does not repo the issue. Unfortunately we're trying to use Unity and Macs for kiosk, events, and installations and it looks horrible when things studder.

Here's hoping Unity will find the time to look into this issue.
