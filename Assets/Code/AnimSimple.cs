using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // #if UNITY_EDITOR

public class AnimSimple : MonoBehaviour
{
    [System.Serializable]
    public class Anim
    {
        public string m_Name = "Walk";
        public float m_FrameRate = 10;
        public bool m_Looping = false;
        public List<Sprite> m_Frames = new List<Sprite>();

        public int Frame { get { return m_Frame; } set { m_Frame = value; } }

        protected int m_Frame;
    }

    public enum AnimType
    { 
        Right_Flip,
        Left_Right,
        Left_Right_Up_Down,
        NonDirectional,
    }

    public AnimType m_AnimType;
    public string m_PrefixMoving = "Walk";
    public string m_PrefixIdle = "Idle";

	protected Anim m_Anim;
    protected float m_Length;
    protected SpriteRenderer m_Sprite;
    protected float m_NextFrame;
    protected bool m_Initialized;
    protected string m_AnimDir;
    protected string m_AnimState;
    protected string m_AnimLast;
    protected bool m_AlwaysFacePlayer;

    protected Vector3 m_Velocity;
    protected Vector3 m_VelocityPlayed;

    public List<Anim> m_Animations = new List<Anim>();
    protected Dictionary<string, Anim> m_QuickList = new Dictionary<string, Anim>();

    public bool Playing { get { return m_Anim != null;  } }
    public float Length { get { return m_Length; } }
    public bool AlwaysFacePlayer { get { return m_AlwaysFacePlayer; } set { m_AlwaysFacePlayer = value; } }

    public bool Play(string animName, bool ignoreWarnings = false )
    {
        InitIfNeeded();

        if (m_QuickList.ContainsKey(animName))
        {
            m_Anim = m_QuickList[animName];
            if (m_Anim.m_FrameRate <= 0)
                m_Length = 0;
            else
                m_Length = (float)m_Anim.m_Frames.Count / m_Anim.m_FrameRate;
            if (m_Anim.m_Looping)
                m_Anim.Frame = (int) (Random.value * m_Anim.m_Frames.Count);
            else
                m_Anim.Frame = 0;
            m_NextFrame = 0;
            m_AnimLast = animName;
            return true;
        }
        else if ( !ignoreWarnings )
		{
            Debug.LogError("Anim " + animName + " not found in " + gameObject.name);
		}

        return false;
    }

    public void SetVelocity(Vector3 vel)
    {
        m_Velocity = vel;
    }

    public bool Play(Vector3 vel)
    {
        switch (m_AnimType)
        {
            case AnimType.Right_Flip:
                if (vel.x < 0)
                    transform.localScale = new Vector3(-transform.localScale.y, transform.localScale.y, transform.localScale.y);
                else
                    transform.localScale = new Vector3(transform.localScale.y, transform.localScale.y, transform.localScale.y);
                break;

            case AnimType.Left_Right:
                if (vel.x > 0)
                    m_AnimDir = "Right";
                else if (vel.x < 0)
                    m_AnimDir = "Left";
                else if (m_AnimDir == null)
                    m_AnimDir = "Right";
                break;

            case AnimType.Left_Right_Up_Down:
                if (vel.x > 0 && Mathf.Abs( vel.x ) >= Mathf.Abs( vel.y ) )
                    m_AnimDir = "Right";
                else if (vel.x < 0 && Mathf.Abs(vel.x) >= Mathf.Abs(vel.y))
                    m_AnimDir = "Left";
                else if (vel.y < 0 )
                    m_AnimDir = "Down";
                else if (vel.y > 0 )
                    m_AnimDir = "Up";
                else if (m_AnimDir == null)
                    m_AnimDir = "Down";
                break;
        }

        if (vel != Vector3.zero )
        {
            m_AnimState = m_PrefixMoving;
        }
        else if (m_PrefixIdle != null && m_PrefixIdle != "")
        {
            m_AnimState = m_PrefixIdle;
        }
        else
        {
            m_AnimState = m_PrefixMoving;
        }

        string animNew = m_AnimState + m_AnimDir;

        if (m_AnimLast == null || m_AnimLast != animNew)
        {
            return Play(animNew);
        }

        return false;
    }
    
    private void InitIfNeeded()
	{
        if (m_Initialized)
            return;

        foreach (Anim anim in m_Animations)
        {
            if (!m_QuickList.ContainsKey(anim.m_Name))
                m_QuickList.Add(anim.m_Name, anim);
        }

        m_Initialized = true;
    }

	// Start is called before the first frame update
	void Start()
    {
        InitIfNeeded();
        m_Sprite = gameObject.GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_VelocityPlayed != m_Velocity)
        {
            m_VelocityPlayed = m_Velocity;
            Play(m_Velocity);
        }

        if ( Time.time >= m_NextFrame && m_Anim != null )
        {
            if (m_Sprite != null )
                m_Sprite.sprite = m_Anim.m_Frames[m_Anim.Frame];

            if (m_Anim.m_FrameRate <= 0)
            {
                m_Anim = null;
                return;
            }
            else
			{
                m_NextFrame = Time.time + (1.0f / m_Anim.m_FrameRate);
            }

            m_Anim.Frame = m_Anim.Frame + 1;

            if (m_Anim.Frame > m_Anim.m_Frames.Count - 1)
            {
                if (m_Anim.m_Looping)
                {
                    m_Anim.Frame = 0;
                }
                else
                {
                    m_Anim = null;
                    return;
                }
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(AnimSimple), false)]
public class EditorAnimSimple : Editor
{
    public override void OnInspectorGUI()
    {
        AnimSimple obj = (AnimSimple)target;

        EditorGUILayout.BeginHorizontal();
        Object dropped = EditorGUILayout.ObjectField("Drag and Drop sprites here:", null, typeof(Object), true);
        if (dropped != null)
        {
            if (dropped as Sprite != null)
            {
                AnimSimple.Anim anim = new AnimSimple.Anim();
                string animName = "";
                foreach ( char c in dropped.name )
				{
                    if (c >= '0' && c <= '9')
                    {
                        if (animName.Length > 0 && animName[animName.Length - 1] == '_')
                            animName = animName.Substring(0, animName.Length - 1);
                        break;
                    }
                    animName = animName + c;
				}
                Debug.Log("Adding AnimSimple.Anim " + animName);
                anim.m_Name = animName;
                for (int i = 0; i < UnityEditor.Selection.count; i++ )
                {
                    Sprite sprite = UnityEditor.Selection.objects[i] as Sprite;
                    if (sprite != null)
                    {
                        Debug.Log("Adding sprite " + sprite + " to " + anim.m_Name);
                        anim.m_Frames.Add(sprite);
                    }
                }
                obj.m_Animations.Add(anim);
            }            
        }
        EditorGUILayout.EndHorizontal();

        base.OnInspectorGUI();
    }
}
#endif // #if UNITY_EDITOR