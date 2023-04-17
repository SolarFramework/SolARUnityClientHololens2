# Release Notes
## v0.12.0

## v0.11.0
* Massive refactor to separate code that can be shared with non Holens 2 Unity projects
* Update SolAR gRPC stubs to support FixedPose and ServiceManager (multi user)
* Add more info in HUD text field (Mapping/Pose status)
* Use UTC UNIX timestamps for sent frames
* Add Playground package for collaborative AR experience in sample scene

## v0.10.0
* Add a console prefab to be able to log error messages
* Display selected selected camera or whether stereo mode is active.
* Delete obsolete files.

## v0.9.0
* Add support for stereo-based mapping using VLC cameras LEFT_FRONT and RIGHT_FRONT.
* Add visual indications for tracking loss.
* Add menu with a collection of objects to build a persitent AR scene.

## v0.8.0
* Performance: move heavy processing (start/stop) outside of UI/main thread.

## v0.7.0
* Change sharing of SpatialCoordinateSystem object between Unity and native plugin so that it works with PV camera as well.
* Make a prefab for the simple GUI.

## v0.6.0
* Fix accuracy issues due the use of different coordinate system origin between Unity application and native plugin: the origin used in Unity is now shared between the two.
* Fix pose errors due to wrong computation for the correction of the pose received from SolAR.
* Add prefabs to display 3D representation of Unity coordinate system axes.

## v0.5.0
* Add reset button
* Send a "stop" request to ARCloud services when application closes (if in "start" mode)

## v0.4.0
* Improve hologram alignment by applying the offset transfom between sensor camera and the eyes location.
* Use a coroutine to start sensors, fetch and send frames + control sending rate
* Add a marker-sized cube to debug registration accuracy

## v0.3.0
* Add JPEG compression option

## v0.2.0
* Add ability to select pipeline mode from relocation AND mapping to mapping only

## v0.1.0
* First release. This plugin provides a prefab which can be used to access SolAR relocalization and mapping in the cloud from a Unity application. 