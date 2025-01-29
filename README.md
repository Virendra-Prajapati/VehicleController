# Unity Vehicle Controller with AI Control

## Summary

This Unity-based vehicle controller script uses Unity's physics engine to simulate realistic vehicle dynamics, offering a versatile solution for racing games, driving simulators, or any game that requires vehicle movement. The controller supports both **player** and **AI** vehicle control, with advanced features like drifting, boosting, realistic steering, deceleration, obstacle detection, and waypoint-based navigation.

## Key Features

- **Advanced drifting & boosting**: Drift your car through corners and boost your speed for an adrenaline-fueled racing experience.
- **Realistic steering & deceleration**: Achieve smooth and responsive vehicle control with real-world physics.
- **AI control with obstacle detection & avoidance**: AI vehicles can detect obstacles in their path and adjust their movement accordingly, ensuring dynamic and intelligent behavior on the track.
- **AI navigation via waypoints**: AI-controlled vehicles follow waypoints, navigating the track smoothly and with decision-making ability to avoid obstacles.
- **Player control via Unity Input System**: Fully supports Unity’s default input system, offering responsive control over your vehicle.

## Demo Video

Check out the gameplay demo video showcasing AI cars racing with obstacle detection and avoiding collisions:

[**Watch Demo Video**](https://github.com/user-attachments/assets/65f1bda5-95f5-4dae-9c93-3a76336b296e)

## Features

- **Wheel collider-based vehicle physics**: Accurate vehicle dynamics using Unity's built-in wheel colliders.
- **Drifting and boosting mechanics**: Control the car's drift behavior and apply speed boosts during gameplay.
- **AI-controlled vehicles with obstacle detection & avoidance**: AI vehicles intelligently navigate the environment, detecting and avoiding obstacles in their path, making the race more dynamic and unpredictable.
- **AI waypoints & reverse handling**: AI-controlled vehicles follow waypoints, with the ability to reverse if stuck, avoid obstacles, and reorient themselves dynamically.
- **Player input via Unity Input System**: Seamlessly integrate with Unity’s default input system for responsive player control.

## Installation

1. **Clone or download the repository**.
2. **Import the script, vehicle models, and assets** into your Unity project.
3. **Set up the vehicle GameObject** with wheel colliders and attach the vehicle controller script.
4. **Attach the AIControl script to the AI vehicle** and link the SplineComputer (track path) and other necessary references in the Inspector.
5. **Configure the AI waypoints, obstacle layers, and sensor distances** as needed for your scene.
6. **Set up the vehicle models and environment to match your scene**.

## Configuration

The **VehicleController** and **AIControl** scripts come with a variety of customizable parameters to suit different types of vehicles and gameplay styles. Here are some of the key configuration options:

### Vehicle Controller Configuration
- **Max speed** (`maxSpeed`): The top speed the vehicle can reach (in units per second).
- **Acceleration** (`acceleration`): How quickly the vehicle accelerates to its max speed.
- **Deceleration** (`deceleration`): How quickly the vehicle slows down when the throttle is released or when braking is applied.
- **Steering sensitivity** (`steeringSensitivity`): Adjusts how sensitive the steering is when turning.
- **Traction control** (`tractionControl`): A multiplier affecting the grip of the vehicle’s tires.

### AI Control Configuration
- **Target layer** (`targetLayer`): Defines the layers that the AI will detect for obstacles (e.g., walls, other cars).
- **Sensor length** (`sensorLength`): Determines how far the AI can "see" when scanning for obstacles.
- **Side sensor distance** (`sideSensorDistance`): How far the side sensors extend from the AI vehicle for collision detection.
- **Side sensor angle** (`sideSensorAngle`): The angle at which side sensors scan for obstacles.
- **Sensor center offset** (`sensorCenterOffset`): Offset from the center of the vehicle where the front and side sensors are located.

### Additional Settings
- **Waypoint radius** (`waypointRadius`): The radius around a waypoint within which the AI will start turning towards the next waypoint.
- **AI max speed** (`aiMaxSpeed`): The top speed of AI-controlled vehicles.
- **Obstacle detection range** (`obstacleDetectionRange`): The range at which the AI will detect and react to obstacles.
- **Obstacle avoidance sensitivity** (`obstacleAvoidanceSensitivity`): How sensitive the AI is to detected obstacles.

## Example Setup

### To set up the AI control with waypoints and obstacle avoidance:

1. **Assign the AIControl script**: Attach the AIControl script to the AI-controlled vehicle.
2. **Connect the SplineComputer**: In the Inspector, link the SplineComputer component that defines the race track path.
3. **Adjust sensor settings**: Fine-tune the sensor length, distance, and angle to make the AI react appropriately to obstacles in your scene.
4. **Configure AI waypoints**: Set up a series of waypoints along the track for the AI to follow.

Feel free to customize and expand the vehicle controller for your own Unity projects. Whether you're building a racing game, a driving simulator, or just testing vehicle dynamics, this controller provides a robust solution with both player and AI vehicle support.
