# Infector

This is a Unity application that simulates limited deployment for A/B testing. In this application, certain users are "coaches" to other users, and we want to keep the software version across coaches and their users. However, this can lead to "total infection" and can lead to too many people having one version of the software. Thus we want to implement "limited infection" so that we have better data set.

The basic philosophy around my implementation is that while normal users may not understand A/B testing, coaches have at least some knowledge and more importantly consistency across coaches and their students who are not coaches is more important than consistency across coaches and their student-coaches.

# How to use

You can move across the graph simply by moving your mouse to the edge of the window to move in that direction. To speed up navigation, one can use the "jump graphs" button to move quickly between independent graphs (users which have no student-coach relationship whatsoever).

To begin infection, click on the user as the source of the infection. To do limited infection, check the "limited" box in the top right.

The application will load a default graph data set. If you wish to use your own data set, you can do so using a JSON file named "data.json". The JSON should be of JSON Array format, with each entry having a field "uid" for the user. There should also be an array "students" with unique ids of students of this user. See "data.json.sample#" for examples.

There are some sample tests that you may build. They all have .sample# file endings.

# Graphing

Because Unity does not come with graphing utilities, I decided to write my own. The graph used here is (mostly) a Directed Acyclic Graph where the a given user's edges represent their students. Breadth First Search is the search algorithm I use for all graph traversal. The graph visualization begins by determining users with no coaches (i.e. entry points to independent graphs). These users are placed around a center with a radius of 2^(max_depth + 1) where max_depth is the greatest distance between the graph's entry point and a given node in that graph. Then for every node, they are rotated around the parent (coach) node with a radius of length_scale * 2^(max_depth - depth - 1) where length_scale is an arbitrary length between the nodes and scales with the exponent. In other words, the closer we get to leaves in the graph, the smaller the distance between users becomes in the visualization. I use Unity's line renderer to draw the arrows that show the direction of edges. Angles are calculated in a sort of "clock" where everytime an angle used, the angle "ticks" a certain number of degrees. This becomes all the more clear once opening the application.

# Testing

There are no formal tests for this application, due to it being in Unity. However, generally the application should follow this logic.

- A "Total infection" will not affect graphs not connected to the original infector.
- Partial infection will infect every other layer of students.
- A layer of students is simply a coach and his students.
- The exception to this rule is if a student of a coach is also a coach.

# Flaws

- Spaghetti code in some areas.
- Navigation is not ideal.
- Testing could be made easier by labeling the users with their UIDs.
    - I would simply have to give instructions on which user to infect when and state the desired color for each user.
- A partial infection of a single class will have the same result as a total infection.
- Another conjecture from the first flaw is that the current partial infection method can still unintentionally infect too many users, depending on the topology of the student-coach relations.
- Despite there being a field for it, this application does NOT support limited infection with quotas.
    - If I were to do this, I would keep a counter of users I have infected.
    - "Best fit" to the remaining number of users to be infected would be the hueristic to determine priority for infection.
