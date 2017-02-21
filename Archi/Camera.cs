using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Archi
{
    public class Camera
    {
        public Vector3 origin;
        public Vector3 forward;
        public Vector3 upward;

        Vector3 intertiaRotate = Vector3.Zero;
        const float rotateSense = 1f / 80f;
        const float fractRotate = 0.3f;

        Vector3 inertTranslate = Vector3.Zero;
        const float translateSense = 10f;
        const float fractTranslate = 0.8f;

        public Camera() {
            origin = new Vector3(30, 3, 0);
            forward = new Vector3(-1, 0, 0);
            upward = new Vector3(0, -1, 0);
        }

        public Vector3 getRight() {
            return Vector3.Cross(upward, forward);
        }

        public Vector3 getLeft() {
            return Vector3.Cross(forward, upward);
        }

        public void smoothRotate(float horiz, float elev, float roll) {
            smoothRotate(new Vector3(horiz, elev, roll));
        }
        public void smoothRotate(Vector3 rotate) {
            intertiaRotate += rotate * rotateSense;
        }

        public void smoothTranslate(float _foward, float _right, float _up) {
            smoothTranslate(new Vector3(_foward, _right, _up));
        }

        public void smoothTranslate(Vector3 trans) {
            inertTranslate += trans * translateSense;
        }

        public void inertCamera() {
            Vector3 partRotate = intertiaRotate * fractRotate;
            rotateLocal(partRotate);
            intertiaRotate -= partRotate;

            Vector3 partTrans = inertTranslate * fractTranslate;
            moveLocal(partTrans);
            inertTranslate -= partTrans;
        }

        public void bindShader(Shader shad) {
            shad.SetVariable("cameraOrigin", origin);
            shad.SetVariable("cameraForward", forward);
            shad.SetVariable("cameraUp", upward);

            Shader.Bind(shad);
        }
        public void rotateLocal(float horiz, float elev, float roll) {
            Matrix3 temp;
            Matrix3.CreateFromAxisAngle(upward, horiz,out temp);
            Matrix3 result = temp;

            Matrix3.CreateFromAxisAngle(getRight(), elev, out temp);
            result = Matrix3.Mult(result, temp);

            Matrix3.CreateFromAxisAngle(forward, roll, out temp);
            result = Matrix3.Mult(result, temp);

            forward = Vector3.Transform(result, forward);
            upward = Vector3.Transform(result, upward);
        }
        public void rotateLocal(Vector3 rotate) {
            rotateLocal(rotate.X, rotate.Y, rotate.Z);
        }

        public void moveLocal(float fowardAmnt, float rightAmnt, float upAmnt) {
            origin += fowardAmnt * forward;
            origin += rightAmnt * getRight();
            origin += upAmnt * upward;
        }
        public void moveLocal(Vector3 move) {
            moveLocal(move.X, move.Y, move.Z);
        }
    }
}
