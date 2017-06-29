; An opening comment
(Another opening comment)
g38.2 x10 f1000 (probe)
G21; Set units to millimeters
G90; Set absolute coordinates
G92 E0; Reset extruder
G28 X0 Y0; Home x and y axis
G0 X30 Y30 F4000; Center extruder above bed
G28 Z0; Home Z axis 
G1 F3000; Set feed rate (speed) for first move

(CIRCLE)
G01 X10 Y30
G02 X30 Y30 I10 J10

(---------- START OF CODE ----------)


G21 (mm)
M6 T1 (Change Tool: Diameter: 1.0000 mm)
M3 (Start Spindle)
M7 (Flood Coolant On)
G0Z0.1000
G0X0.0000Y0.0000Z0.1000
G0 X5.0000Y10.0000
G1 Z-2.0000F5.00
F5.00
G2 X5.0000Y10.0000   i4.0000 j0   z-2.0000
G0Z0.1000 (Retract)
M9 (Coolant Off)
M5 (Stop Spindle)
G0X0Y0 (Origin)
M30 (End of Program)

(---------- END OF CODE ----------)