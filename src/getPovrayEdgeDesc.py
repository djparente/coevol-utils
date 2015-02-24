#!/cygdrive/c/Python27/python.exe
# 
# Daniel J. Parente
# MD/PhD Candidate
# University of Kansas Medical Center
# 2013-07-11
#
# Calculate a PovRay scene file to show high-scoring edges on a backbone
# trace of the structure
#
# Status: Complete
# Version: 1.0
# Language: Python
# Changelog:
#    + 2013-07-11: Created

import sys
import math
import itertools

class Residue:
	def __init__(self, descStr):
		fields = descStr.strip().split('\t')
		if len(fields) != 7:
			raise ValueError('Wrong number of fields in a residue descriptor string')
		
		self.lac = str(fields[0])
		self.a = Point3D(fields[1], fields[2], fields[3])
		self.b = Point3D(fields[4], fields[5], fields[6])
		
	def __repr__(self):
		return "Residue " + self.lac + " at " + str(self.a) + " and " + str(self.b)
	
class Edge:
	def __init__(self, descStr):
		fields = descStr.strip().split('\t')
		if len(fields) != 3:
			raise ValueError('Wrong number of fields in an edge descriptor string')
		self.res1 = str(fields[0])
		self.res2 = str(fields[1])
		self.weight = float(fields[2])
	
class Point3D:
	def __init__(self, x, y, z):
		self.x = float(x)
		self.y = float(y)
		self.z = float(z)
		
	def __repr__(self):
		return "<" + str(self.x) + ", " + str(self.y) + ", " + str(self.z) + ">"
	
	def __add__(self, other):
		return Point3D(self.x + other.x, self.y + other.y, self.z + other.z)
		
	def __sub__(self, other):
		return Point3D(self.x - other.x, self.y - other.y, self.z - other.z)
	
	def __iadd__(self, other):
		self.x += other.x
		self.y += other.y
		self.z += other.z
		return self
	
	def mulBy(self, scalar):
		return Point3D(self.x*scalar,self.y*scalar, self.z*scalar)
		
	def scale(self, scalar):
		self.x *= float(scalar)
		self.y *= float(scalar)
		self.z *= float(scalar)
		
		return self
		
	def magnitude(self):
		return math.sqrt(self.x*self.x + self.y*self.y + self.z*self.z)
	
	def normalize(self):
		return self.scale(1.0 / self.magnitude())
		
	@staticmethod
	def crossProduct(a, b):
		return Point3D(a.y * b.z - b.y * a.z, -(a.x * b.z - b.x * a.z), a.x * b.y - b.x * a.y)
		
	@staticmethod
	def dotProduct(a, b):
		return a.x*b.x + a.y*b.y + a.z*b.z
	
	@staticmethod
	def getRotation(a,b):
		axis = Point3D.crossProduct(a,b).normalize()
		angle = math.acos(Point3D.dotProduct(a,b)/(a.magnitude() * b.magnitude()))*180.0/math.pi
		return (axis, angle )
		
	@staticmethod
	def distance(a,b):
		return (b-a).magnitude()

def centroid(chain):
	center = Point3D(0, 0, 0);
	
	for point in chain:
		center += point
	
	center.scale(1.0 / float(len(chain)))
	
	return center;
	
def printProteinChainCylinder(a, b):
	print str.format("    cylinder {{ {0}, {1}, .3 pigment {{ proteinChainColor }} }}", a, b)
	print str.format("    sphere {{ {0}, .3 pigment {{ proteinChainColor }} }}", a)

def printEdgeCylinder(a, b):
	print str.format("    cylinder {{ {0}, {1}, .15 pigment {{ rgb <1, 0, 0> }} }}", a, b)
	
def getClosestApproach(res1, res2):
	approaches = sorted([ (Point3D.distance(x,y), x, y) for x, y in itertools.product([res1.a], [res2.a, res2.b])])
	
	return approaches[0]
	
def determinePovLines(structure, edges):
	resDict = { x.lac : x for x in structure }
	
	aChain = [x.a for x in structure]
	bChain = [x.b for x in structure]
	
	povHeader = "#include \"transforms.inc\"\n#include \"colors.inc\"\n\nglobal_settings { assumed_gamma 1 }\n background { rgb 1 }\n\n#declare backgroundColor=rgb 1;\n#declare proteinChainColor=rgbt <.85,.85,.85,0>;\n\n\nbox {\n    <1,1,1>*2000, <-1,-1,-1>*2000 \n    pigment\n    {\n       backgroundColor\n    }\n    finish\n    {\n        ambient 1\n        diffuse 0\n    }\n    hollow on\n}";

	lines = []
	aCent = centroid(aChain)
	bCent = centroid(bChain)
	dimerCent = (aCent + bCent).scale(0.5)
	
	nCent = (centroid(aChain[0:60]) + centroid(bChain[0:60])).scale(.5);
	cCent = (centroid(aChain[61:len(aChain)-1]) + centroid(bChain[61:len(bChain)-1])).scale(.5);
	
	# Determine the axis
	sAxis = (nCent - cCent).normalize()
	mAxis = (bCent - aCent).normalize()
	cAxis = Point3D.crossProduct(sAxis, mAxis).normalize()
	
	# Get a sense of scale
	mirrorPoints = [ (a + b).scale(0.5) for a, b in zip(aChain, bChain) ]
	maxDist = max([Point3D.distance(a,b) for a, b in itertools.product(mirrorPoints, mirrorPoints)])
	
	lookAt = dimerCent
	up = sAxis
	cameraLoc = cAxis.scale(maxDist*7.5)
	
	# Write header data, set up the camera and lighting
	print povHeader
	print "camera {"
	print str.format("    perspective")
	#print str.format("    location {0!r}", cameraLoc)
	print str.format("    location {0!r}*z", cameraLoc.magnitude())
	
	#print str.format("    look_at {0!r}", lookAt)
	print str.format("    look_at 0")
	print "    up -image_height/image_width*y"	#Negative to get correct chirality
	print "    angle 18.0"
	
	
	print "}"
	print str.format("light_source {{ {0!r}*z, 1 }}", cameraLoc.magnitude())
	
	print "union {"
	
	# Write the chains
	# Chain A
	print "merge {"		# Merge might be a slightly better choice, but is much slower
	for i in range(len(aChain) -1):
		printProteinChainCylinder(aChain[i], aChain[i+1])
	print "}"

	#Chain B
	print "merge {"
	for i in range(len(bChain) -1):
		printProteinChainCylinder(bChain[i], bChain[i+1])
	print "}"
	
	# Write the edges
	involvedNodes = []
	print "union {"
	for edge in edges:
		res1 = resDict[edge.res1]
		res2 = resDict[edge.res2]
		
		closest = getClosestApproach(res1, res2)
		printEdgeCylinder(closest[1], closest[2])
		
		involvedNodes.append(closest[1])
		involvedNodes.append(closest[2])
	print "}"
	
	# Put red spheres at co-evolving nodes
	involvedNodes = set(involvedNodes)
	print "union {"
	for node in involvedNodes:
		print str.format("    sphere {{ {0!r}, .51 pigment {{ color rgb<1, 0, 0> }} }}", node)
	print "}"
	# Debugging: Plotting some key points: the mirror axis across the dimer and various centroids
	#   Not needed for final figures, so commented out
	#print "union {"
	#for p in mirrorPoints:
	#	print str.format("    sphere {{ {0!r}, .1 pigment {{ color rgb<0, 1, 0> }} }}", p)
		
	#print str.format("    sphere {{ {0!r}, .5 pigment {{ color rgb<0, 1, 0> }} }}", nCent)
	#print str.format("    sphere {{ {0!r}, .5 pigment {{ color rgb<0, 1, 0> }} }}", cCent)
	#print str.format("    sphere {{ {0!r}, .5 pigment {{ color rgb<1, 1, 0> }} }}", dimerCent)
	#print "}"
	
	# Rotate the screw axis onto the Y axis
	print str.format("translate -{0!r}", dimerCent)
	axis, angle = Point3D.getRotation(Point3D(0, 1, 0), sAxis)
	print str.format("Axis_Rotate_Trans( {0!r} , {1})", axis, -angle)
	
	# Calulcate the impact of the last rotation on the inter-monomer axis
	cAng = math.cos(-angle*math.pi/180.0)
	sAng = math.sin(-angle*math.pi/180.0)
	mAxisRot = mAxis.mulBy(cAng) + Point3D.crossProduct(axis, mAxis).mulBy(sAng) + axis.mulBy(Point3D.dotProduct(axis, mAxis) * (1.0-cAng))	# This is Rodrigues' rotation formula
	
	# Set the intermonomer axis to point along X by rotating around the Y axis
	axis = Point3D(0, 1, 0)
	angle = math.acos(Point3D.dotProduct(mAxisRot,Point3D(1, 0, 0))/(mAxisRot.magnitude() * axis.magnitude()))*180.0/math.pi
	print str.format("Axis_Rotate_Trans( {0!r} , {1})", axis, -angle)
	
	print "}"
	
def main():
	atomPath = sys.argv[1]
	residues = [Residue(line) for i, line in enumerate(open(atomPath)) if i > 0]
	edges = [ Edge(line) for line in sys.stdin ]
	
	#print "\n".join(map(str,residues))
	
	determinePovLines(residues, edges)

main()