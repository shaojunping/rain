///Update 2017.3.8 smooth wave && Disactive body

using UnityEngine;
using System;
//using System.Collections;

public class PlantsCollision : MonoBehaviour {
    private Material originalMaterial;

    public float duration = 1.0f;
    public float forceScale = 1.0f;

    public bool usePlayerForce = false; // should force continuse?
    public bool useWindForce = false;

    public GameObject customForceObj;//special obj of force,Only One!
    private Vector3 windDirection;
    private float susForceScale = 1.0f;
    private float playerWeight = 0.0f;
    private GameObject currentPlayer;
    PlantsCollisionPra customForcePra;

    private Renderer myRenderer;
    private bool touched = false;
    private bool doubletouched = false;
    Vector4 newCollisionVec;
    float CollisionWeight;

    private bool left = false;
    private bool finished = true;
    private bool left1 = false;
    private bool finished1 = true;

    private float touchBending = 0.0f;
    private float targetTouchBending = 0.0f;
    private float easingControl = 0.0f;

    private float touchBending1 = 0.0f;
    private float targetTouchBending1 = 0.0f;
    private float easingControl1 = 0.0f;
 
    private float timer = 0.0f;
    private float timer1 = 0.0f;

    private float timeLeft;

    private int Player_ID, Player1_ID;
    
	// Use this for initialization
	void OnEnable () {
        myRenderer = GetComponent<Renderer>();
      
        originalMaterial = myRenderer.sharedMaterial;
        //Debug.Log("Share Mat:" + originalMaterial.name);
        //myRenderer.sharedMaterial = originalMaterial;
        if (usePlayerForce)
            useWindForce = false; 
	}

    void OnDisable()
    {
        AllOver();
    }

    void OnTriggerEnter(Collider other)
    {
        //If we use custome force Obj,we do not calclute touching of other trigger obj!
        if (customForceObj)
            if (other.gameObject != customForceObj.gameObject)
                return;

        if (!touched)
        {
            Player_ID = other.GetInstanceID();
            timer = 0.0f;
            //timeIn = 0.0f;
            touched = true;
            left = false;
            targetTouchBending = 1.0f;
            touchBending = 1.0f;
            finished = false;
            playerWeight = 1.0f;
            
        }
        else
        {
            //if we have already got 2 players,the 2nd player will be the 1st one,and the new player will be the 2nd player
            if (doubletouched == true)
            {
                SwapTouchBending();
            }
            Player1_ID = other.GetInstanceID();
            timer1 = 0.0f;
            left1 = false;
            targetTouchBending1 = 1.0f;
            //touchBending1 = targetTouchBending1;
            touchBending1 = 1.0f;
            finished1 = false;
            doubletouched = true;
        }
        // the last obj is the current Player,if it disactive,All over!
        currentPlayer = other.gameObject;
        customForcePra =currentPlayer.GetComponent<PlantsCollisionPra>();

    }

    void OnTriggerExit(Collider other)
    {
        if (customForceObj)
            if (other.gameObject != customForceObj.gameObject)
                return;

        if (Player_ID != Player1_ID)
        {
            // which one do we have to set?
			if (other.GetInstanceID() == Player_ID) {
				left = true;
				targetTouchBending = 0.0f;
			}
			else {
				left1 = true;
				targetTouchBending1 = 0.0f;	
            }
        }
        else {
			left = true;
			targetTouchBending = 0.0f;	
			left1 = true;
			targetTouchBending1 = 0.0f;		
		}
        //if (left)
        //    Debug.Log("!!!!!!!!!!!!!LEFT!!!!!!!!!!!!!!!!!");

        timeLeft = 0.0f;
    }
	// Update is called once per frame
	void Update () {
        //Does sth touch my grass?
        if (!touched)
        {
            return;
        }


        touchBending = Mathf.Lerp(touchBending, targetTouchBending, (timer) / duration);
        easingControl = Bounce(timer);
        playerWeight = Mathf.Lerp(0.0f, 1.0f, timer * 2);

        //if double touching,we calculate 2nd Player,or else we calculate 1st Player
        #region only 1 Player
        //////////////////////////////////
        if (!doubletouched) 
        {
            if (finished && targetTouchBending == 0 && timeLeft >=1)
            {
                AllOver();
                return;
            }

            if (currentPlayer == null || !currentPlayer.activeSelf)
            {
                AllOver();
                return;
            }

            newCollisionVec.w = easingControl * forceScale;
            // if 1st player left,palyer force will calm down from 1 to 0
            if (left)
            {
                playerWeight = Mathf.Lerp(1.0f, 0.0f, timeLeft);
                timeLeft += 4 * Time.deltaTime;
            }
            //Debug.Log("playerWeight:" + playerWeight);
            /////////////////
            #region Use Player Force
            if (customForcePra && usePlayerForce)
            {
                useWindForce = false;
                susForceScale = customForcePra.playerForceScale;
                Vector4 playerF = new Vector4(currentPlayer.transform.position.x, currentPlayer.transform.position.y, currentPlayer.transform.position.z, susForceScale);
                myRenderer.material.SetVector("_PlayerForce", playerF);

                //we cant change fre! Or else when finishing touching  it will jitter!
                if (customForcePra.windShareMat)
                    myRenderer.sharedMaterial.SetFloat("_ForceWeight", playerWeight);
                else
                    myRenderer.material.SetFloat("_ForceWeight", playerWeight);
                    //Debug.Log("_ForceWeight:" + myRenderer.material.GetFloat("_ForceWeight"));
            }
            #endregion
                myRenderer.material.SetFloat("_ColliderForce", newCollisionVec.w);
                //Debug.Log("_ColliderForce:" + myRenderer.material.GetFloat("_ColliderForce"));
                //Debug.Log("myRenderer Mat:" + myRenderer.material.name);

            ///////////////////
            #region wind mode
            if (useWindForce)
            {
                usePlayerForce = false;
                Vector4 playerW = new Vector4(windDirection.x, windDirection.y, windDirection.z, susForceScale);

                if (customForcePra)
                { 
                    playerW.x = customForcePra.windDir.x;
                    playerW.z = customForcePra.windDir.z;

                    if (customForcePra.windShareMat)
                    {
                        myRenderer.sharedMaterial.SetVector("_DirForce", playerW);
                        myRenderer.sharedMaterial.SetFloat("_WindEdgeFlutterFreqScale", customForcePra.forceFre);
                    }
                    else
                    {
                        myRenderer.material.SetVector("_DirForce", playerW);
                        myRenderer.material.SetFloat("_WindEdgeFlutterFreqScale", customForcePra.forceFre);
                    }
                }
                //no self weight
                newCollisionVec.w = 0;
            }
            #endregion
            //////////////////

            timer += Time.deltaTime;

        }
        #endregion
        //////////////////////////////////
        #region 2 players
        else 
        {
            //we only calculate 2nd Player!!! No matter the 2nd Player left or stay!!!
            #region 1st left,swap 2nd --->1st
            //if 1st Player left,we swap 2nd to 1st Player and calculate 1st!
            //important: at that time Player ID2 -------->ID1
            if (finished && targetTouchBending == 0.0f)
            {
                SwapTouchBending();
                doubletouched = false;
                touchBending = Mathf.Lerp(touchBending, targetTouchBending, (timer) / duration);
                easingControl = Bounce(timer);

                if (finished && targetTouchBending == 0.0f && timeLeft >= 1)
                {
                    // if second touch bending animation has ended at the same time
                    AllOver();
                    return;
                }

                if (currentPlayer == null || !currentPlayer.activeSelf)
                {
                    AllOver();
                    return;
                }

                //else
                //{
                newCollisionVec.w = easingControl * forceScale;
                if (left)
                {
                    playerWeight = Mathf.Lerp(1.0f, 0.0f, timeLeft);
                    timeLeft += 4 * Time.deltaTime;
                }

                ////////////
                #region player mode 
                if (usePlayerForce && customForcePra)
                {
                    //newCollisionVec.w = 0.5f * newCollisionVec.w;
                    useWindForce = false;
                    susForceScale = customForcePra.playerForceScale;
                    Vector4 playerF = new Vector4(currentPlayer.transform.position.x, currentPlayer.transform.position.y, currentPlayer.transform.position.z, susForceScale);
                    myRenderer.material.SetVector("_PlayerForce", playerF);
                    if (customForcePra.windShareMat)
                        myRenderer.sharedMaterial.SetFloat("_ForceWeight", playerWeight);
                    else
                        myRenderer.material.SetFloat("_ForceWeight", playerWeight);
                    //Debug.Log("ForcePos:" + playerF);
                }
                #endregion
                ////////////
                myRenderer.material.SetFloat("_ColliderForce", newCollisionVec.w);
                timer += Time.deltaTime;
                //}
            }
            #endregion 

            #region 2nd left,calculate 2nd!
            //if 2nd Player left,we calculate 2nd Player!
            else 
            {
                touchBending1 = Mathf.Lerp(touchBending1, targetTouchBending1, timer1 / duration);
                easingControl1 = Bounce1(timer1);
                if (finished1 && targetTouchBending1 == 0.0f)
                {
                    doubletouched = false;
                    return;
                }
                //else
                //{
                if (currentPlayer == null || !currentPlayer.activeSelf)
                {
                    AllOver();
                    return;
                }

                newCollisionVec.w = easingControl1 * forceScale;

                if (left1)
                {
                    playerWeight = Mathf.Lerp(1.0f, 0.0f, timeLeft);
                    timeLeft += 4 * Time.deltaTime;
                }
                ////////////
                #region player mode 
                if (usePlayerForce)
                {
                    //newCollisionVec.w = 0.5f * newCollisionVec.w;
                    useWindForce = false;
                    if (customForcePra)
                        susForceScale = customForcePra.playerForceScale;
                    Vector4 playerF = new Vector4(currentPlayer.transform.position.x, currentPlayer.transform.position.y, currentPlayer.transform.position.z, susForceScale);
                    myRenderer.material.SetVector("_PlayerForce", playerF);

                    useWindForce = false;
                    susForceScale = customForcePra.playerForceScale;

                    if (customForcePra.windShareMat)
                        myRenderer.sharedMaterial.SetFloat("_ForceWeight", playerWeight);
                    else
                        myRenderer.material.SetFloat("_ForceWeight", playerWeight);

                    //Debug.Log("ForcePos:" + playerF);
                }
                #endregion
                ////////////

                //Debug.Log("Control:" + newCollisionVec.w);
                myRenderer.material.SetFloat("_ColliderForce", newCollisionVec.w);
                timer1 += Time.deltaTime;
                //}
            }
            #endregion

        }
        #endregion
        //////////////////////////////////

        }
        

    public float Bounce(float x)
    {
        if ((x / duration) >= 1f)
        {
            if (easingControl == 0.0f && left == true)
            {
                finished = true;
            }
            return targetTouchBending;
        }
        //return Mathf.Lerp(Mathf.Sin(x * 10.0f / duration) / (x + 1.25f) * 8.0f, touchBending, Mathf.Sqrt(x / duration));
        return Mathf.Lerp(Mathf.Sin(x * 10.0f / duration) / (x + 1.25f) * 8.0f, touchBending, x / duration);
    }

    public float Bounce1(float x)
    {
        if ((x / duration) >= 1f)
        {
            if (easingControl1 == 0.0f && left1 == true)
            {
                finished1 = true;
            }
            return targetTouchBending1;
        }
        return Mathf.Lerp(Mathf.Sin(x * 10.0f / duration) / (x + 1.25f) * 8.0f, touchBending1, x / duration);
    }

    public void AllOver()
    {
        if (myRenderer.material == null)
            return;
        if (myRenderer.material != originalMaterial)
        {
            try
            {
				GameObject.Destroy(myRenderer.material);
            }
            catch (Exception e) {
                Debug.Log("<color=blue>PlantsCollision.DestroyImmediate,  e=" + e.Message + "</color>");
            }
            myRenderer.sharedMaterial = originalMaterial;
        }
        touched = false;
        doubletouched = false;
        currentPlayer = null;

    }

    public void SwapTouchBending()
    {
        Player_ID = Player1_ID;
        touchBending = touchBending1;
        targetTouchBending = targetTouchBending1;
        easingControl = easingControl1;
        left = left1;
        finished = finished1;
        timer = timer1;
    }

}
