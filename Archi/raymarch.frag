#version 400

#include lib.frag

uniform vec2 viewportSize;
uniform vec3 cameraOrigin;
uniform vec3 cameraForward;
uniform vec3 cameraUp;
layout(pixel_center_integer) in vec4 gl_FragCoord;
out vec4 pixelColor;

// Color

float colormap_red(float x) {
    if (x < 0.5) {
        return -6.0 * x + 67.0 / 32.0;
    } else {
        return 6.0 * x - 79.0 / 16.0;
    }
}

float colormap_green(float x) {
    if (x < 0.4) {
        return 6.0 * x - 3.0 / 32.0;
    } else {
        return -6.0 * x + 79.0 / 16.0;
    }
}

float colormap_blue(float x) {
    if (x < 0.7) {
       return 6.0 * x - 67.0 / 32.0;
    } else {
       return -6.0 * x + 195.0 / 32.0;
    }
}

vec4 colormap(float x) {
    float r = clamp(colormap_red(x), 0.0, 1.0);
    float g = clamp(colormap_green(x), 0.0, 1.0);
    float b = clamp(colormap_blue(x), 0.0, 1.0);
    return vec4(r, g, b, 1.0);
}

//End color


float level1(vec3 p) {
	float d;
	float plane = fPlane(p,vec3(0,1,0),14);

	pModPolar(p.xz,8);
	
	pMirrorOctant(p.xz,vec2(120,65));
	pMirrorOctant(p.xz,vec2(40,60));
	
	float coord = pModMirror1(p.x,200);
	pModInterval1(p.y,30,0,2);

	p.x = -abs(p.x) + 27;
	float c = pMod1(p.z,20);
	float wall = fBox2(p.xy,vec2(1,15));
	p.z = abs(p.z)-3;
	p.z = abs(p.z)+2;
	float box = fBox(p,vec3(3,9,4));
	p.y-=9;
	float cylinder = fCylinder(p.yxz,4,3);
	p.y -= 6;
	
	pR(p.xy,0.5);
	p.x -= 18;
	float roof = fBox2(p.xy,vec2(20,0.5));
	float window = min(box, cylinder);
	d = fOpDifferenceColumns(wall,window,0.6,3);
	d = min(d,roof);
	d= fOpUnionRound(d,plane,10);
	return d;
}

float level2(vec3 p) {

	vec3 other = pMod3(p,vec3(500));



	float outterS = fSphere(p,110);
	float midS = fSphere(p,300);
	float inS = fSphere(p,270);
	float plane = fPlane(p,vec3(0,1,0),0);
	float carve = max(midS,-inS);
	//carve = max(carve,-plane);

	 vec3 coords = pMod3(p,vec3(10));
	 float boxes = fBox(p, vec3(4));

	carve = fOpDifferenceStairs(carve,boxes,3,3);
	carve = max(carve,-boxes);
	float d = carve;
	//d=boxes;
	return d;
}

float level3(vec3 p) {
	float segment = pModPolar(p.xz,30);
	p.x-=100;
	
	float index = pModSingle1(p.x,100);
	float hex = fBox(p,vec3(30,50+50,30));

	return hex;
}

float map(vec3 p) {
	
	return level2(p);
}


vec3 calcNormal(vec3 p) {
	float e = 0.1;
	vec3 n = vec3(map(p+vec3(e,0,0)) - map(p-vec3(e,0,0)),map(p+vec3(0,e,0))-map(p-vec3(0,e,0)),map(p+vec3(0,0,e))-map(p-(0,0,e)));
	return normalize(n);

}

float shadow(vec3 ro, vec3 rd, float mint, float maxt, float k, int maxIt) {
	float res = 1.0;
	float t = mint;
	for(int i = 0; i<maxIt;++i) {
		float h = map(ro + rd*t);
		if(h<0.01) {
			return 0.0;
		}
		res = min(res,k*h/t);
		t+=h;
	}
	return res;
}

void main()
{
    //pixelColor = position;
//	pixelColor =  vec4(gl_FragCoord.xyz,1.0);
	//pixelColor = vec4(1.0,0,0,1.0);
	float aspect = viewportSize.y/viewportSize.x  ;
    vec2 screenSpace = vec2((gl_FragCoord.x/viewportSize.x)*2 - 1,-((gl_FragCoord.y/viewportSize.y)*2-1)*aspect);
	
	pixelColor = vec4(screenSpace,0.0,1.0);
	
	float fov = PI/2.0;
	float cameraDist = 1.0/(atan(fov/2.0));
	
    
    vec3 cameraRight = cross(cameraUp, cameraForward);//vec3(0.0,1.0,0.0);
    vec3 cameraFocual = cameraOrigin + cameraForward * cameraDist;
    vec3 cameraPoke = cameraFocual +screenSpace.x * cameraRight+ screenSpace.y * cameraUp;

	vec3 rayOrig = cameraOrigin;
	vec3 rayDir = normalize(cameraPoke - cameraOrigin);

	float t_min = 0.1;
	float t_max = 30000;
	float pixelError = 0.1;
	bool forceHit = false;
	int MAX_ITERATIONS = 64;
	
	float omega = 1.2;
		
	vec3 inspect = rayOrig;
	float runningDist = 0.0;
	bool found = false;
	pixelColor = vec4(1,1,1,1);
	int maxDepth = 128;
	bool first = false;
	for(int i=0;i<maxDepth;i++) {
		inspect = rayOrig + runningDist * rayDir;
		float dist = map(inspect);
		if(dist<=0.01 || (i==maxDepth-1 && dist<0.1) ) {
			found = true;
			dist += dist;
			if(i==0){
			first = true;
			}
			break;
		}
		if(dist>10000) {
			break;
		}
		runningDist += dist;
	}
	pixelColor = vec4(calcNormal(rayOrig+rayDir*runningDist),1);
	vec3 n = calcNormal(inspect);
	float incident = dot(n,rayDir);

	vec3 color = vec3(1,1,1);
	vec3 sunDir = normalize(vec3(0.5,0.5,0));
	
	float shad = 1;//shadow(inspect, sunDir,5,200,2,64);

	float diffuse = dot(n,sunDir) * shad;
	float specular =  dot(reflect(rayDir,n),sunDir)*0.3 * shad;
	float ambient = 0.2;
	pixelColor = colormap(mod(length(inspect)*5,1));


	color =vec3(0);
	color += diffuse * vec3(0.4,0.8,1);
	color += specular * normalize(vec3(1,1,1));
	color += ambient * vec3(1,0,0);

	color = color / 1.8;

	//color = normalize(vec3(1)) * diffuse * shad;

	if(incident==0) {
		color = vec3(1,0,0);
	}
	if(first) {
		color = vec3(0,1,1);
	}


	pixelColor = vec4(color,1);
	//pixelColor = vec4(5-vec3(runningDist),1);
	//pixelColor = vec4(runningDist/30,dot(n,vec3(0.5,0.5,0)),dot(reflect(rayDir,n),vec3(0.5,0.5,0)),1);
	//pixelColor = clamp(pixelColor,vec4(0),vec4(1));
	if(!found) {
		pixelColor = vec4(0,0.8,0.3,1);
	}
	//pixelColor = vec4(rayDir.x,rayDir.y,rayDir.z,1);
}
