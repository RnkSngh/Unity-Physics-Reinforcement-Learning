# Unity-Physics-Reinforcement-Learning
This folder contains two separate Unity projects - a 2d inverted pendulum, and a robot arm, that explore simulating reinforcement learning agents using Unity as a physics engine. Though the physics are simulated in Unity, neither agents utilize any physics knowledge (i.e. neither use closed form solutions to accomplish their respective tasks) and instead use some arbitrary mapping of observations to actions that was learned through training. Both agents use Proximal Policy Optimization as there was a non-sparse reward given (i.e. the reward was given at each timestep). both units utilize 128 units over 2 hidden layers. 
## Table of Contents

## Untrained 2D Inverted Pendulum
![](/GameGifs/2d_pendulum-untrained.gif)

## Trained 2D Inverted Pendulum
![](/GameGifs/2d_pendulum-trained.gif)
## Untrained Robot Arm
![](/GameGifs/robo_arm_untrained.gif)
## Trained Robot Arm
![](/GameGifs/robo_arm_trained.gif)
# 2D Inverted Pendulum
The 2D inverted Pendulum problem is a generalization of the [inverted pendulum problem](https://www.youtube.com/watch?v=ycsYhmwX9lM) - where a square balances a weight connected  to it with a hinge joint, and alsoto stay within a given distance. Instead of a rectangle being constrained to move in one dimension, the 2D generalization allows for movement of a cube in two dimensions. 


## Reward Function
The reward function this neural net was trained to maximize was the y position of the weight that it was trying to balance. This follows from the fact that the hiehgt of the rod is maximized when the rod is perfectly balanced.
## Observations 
The neural net was given a 10 dimensional input. 4 of the 10 input dimensions correspond to teh x and z coordinates of the position and velocity of the bottom cube (y was neglected as the cube cannot move in the y direction). The other 6 inputs come from the x, y, and z positions and velocities of the weight that is being balanced. The coordinate system of the weight being balanced was taken to be wrt to the cube, as only the positions and velocitires relative to the cube are relevant. Thus, the inputs for the positions asnd velocities first subtracted the positions and velocities of the cube before being sent as training inputs to the value predicting neural net. 
## Actions
The output of the agent was a normalized 2 dimensional vector, where each dimension represents the force to be applied in the x and z directions, which were then scaled up by a constant factor of 25 newtons that would be applied to the base of the cube. Only the cube itself was controlled, the position and velocity of the balanced weight was controlled by the connection between teh rod to the weight, whcih was inturn controlled by the connection to the rod and the cube. 
## Reset Conditions
The unity ml agents allows to specify conditions when an episode can be reset. The reset conditions were chosen to be when either the y component of the weight was negative (i.e. the ball went below the cube) or if the cube's x or y position moved outside of a 10x10 square centered on the origin. 

# Robot Arm
The Robot arm directory contains a robot arm which was trained to move to a target cube with a randomly generated position within the arm's reachable radius (because this arm can be modeled 2 rods connected by a shoulder and elbow, or in other words a ball and socket joint and hinge joint, the radius of all possible positions, allowing the joints to overlap, is a sphere (if the problem was of two hinge joints the shape might be a torus instead of a sphere). No deformations of any of the components of the arm were modeled, and thus all possible movements of the arm can be modeled as a set of independently applied torques to both of the joints. This task was inspired by the unity robot arm on youtube, but was a force-based actuation instead of a position base dactuation - thus this simulation allows for more realistic prediction of gravity, rather than one which is only constrained to move in precice directions. 
## Reward Function
The reward function to be maximized was the negative of the distance between the target cube and the hand. 
## Observations 
The 16 dimensional observation was given to the agent waere the x, y, and z positions and velocities of the hand, the x, y, and z positions and velocities of the elbow with respect to the hand, and a quaternion representing the orientation of the shoulder (ball and socket) joint. 
## Actions
The output for the agent was a 4d vector corresponding to the torque being applied to rotate the elbow joint in the 1 angular direction it was free to rotate in, and the 3 angular directions that the shoulder joint could rotate in. 
## Reset Conditions
The training episode was reset once the arm reached the goal, which was calculated by checking if the hand was within a gien tolerance distance (chosen to be 0.7 meters) within the center of the target cube.

