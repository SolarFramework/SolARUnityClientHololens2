# Release Notes

# v0.7.0
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