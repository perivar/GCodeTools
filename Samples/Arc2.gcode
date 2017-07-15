(File built with GCodeTools)
(Generated on 11.07.2017 12.54.55)

(Header)
G90 (set absolute distance mode)
G17 (set active plane to XY)
G40 (turn cutter compensation off)
G21 (set units to mm)
G0 Z2
M3 S4000 (start the spindle clockwise at the S speed)
(Header end.)

G0 X0 Y0 Z1

G0 X10 Y-20
G1 Z-1 F5
G2 X10 Y40 Z-1 I20 J30
G0 Z1 (Retract)

G0 X110 Y-20
G1 Z-1 F5
G2 X110 Y40 Z-1 I20 J30
G0 Z1 (Retract)

G0 X0 Y0 (Origin)

(Footer)
M5 (stop the spindle)
(Footer end.)

