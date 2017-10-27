using UnityEngine;

namespace ZLib
{
    //UV 动画
    public class UVAni : MonoBehaviour
    {
        //x方向速度
        public float fUVMoveSpeedX = 2;
        //y方向速度
        public float fUVMoveSpeedY = 2;

        //每一帧的偏移量
        Vector2 offset = new Vector2();
        //对象上的Render组件对象
        Renderer render = null;

        // Use this for initialization
        void Start()
        {
            render = GetComponent<Renderer>();

            if (render == null
                || render.material == null
                || !render.material.HasProperty("_MainTex"))
                enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            //if (render == null
            //    || render.material == null
            //    || !render.material.HasProperty("_MainTex"))
            //{
            //    enabled = false;
            //    return;
            //}

            offset.x += Time.deltaTime * fUVMoveSpeedX;
            offset.y += Time.deltaTime * fUVMoveSpeedY;

            render.material.mainTextureOffset = offset;
        }

    }
}
