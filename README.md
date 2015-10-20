# Infector

This is a Unity application that simulates limited deployment for A/B testing. In this application, certain Users are "coaches" to other Users, and we want to keep the software version across coaches and their users. However, this can lead to "total infection" and can lead to too many people having one version of the software. Thus we want to implement "limited infection" so that we have better data set.

The basic philosophy around my implementation is that while normal users may not understand A/B testing, coaches have at least some knowledge and more importantly consistency across coaches and their students who are not coaches is more important than consistency across coaches and their student-coaches.

# How to use

You can move across the graph simply by moving your mouse to the edge of the window to move in that direction. To speed up navigation, one can use the "jump graphs" button to move quickly between independent graphs (users which have no student-coach relationship whatsoever).

To begin infection, click on the user as the source of the infection. To do limited infection, check the "limited" box in the top right.

The application will load a default graph data set. If you wish to use your own data set, you can do so using a JSON file named "data.json". The JSON should be of JSON Array format, with each entry having a field "uid" for the user. There should also be an array "students" with unique ids of students of this user. See data.json.sample for an example.

There are some sample tests that you may build. They all have .sample# file endings.
