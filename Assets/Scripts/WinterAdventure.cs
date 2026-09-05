using UnityEngine;
using UnityEngine.InputSystem;

// Presentation and progression for the Winter Summit scene; movement stays in PlayerMovement.
[RequireComponent(typeof(PlayerMovement))]
public class WinterAdventure : MonoBehaviour
{
    public Camera sceneCamera;
    public Transform backdrop, artwork, scarf, summit;
    public Transform[] snowflakes, lights;
    public AudioClip wind, jump, landing, footstep, collect, finish;
    private AudioSource ambience, effects;
    private Rigidbody2D body;
    private Vector3 start, cameraStart;
    private Vector3[] lightStarts;
    private float maxHeight, lastStep, previousVelocity, cameraVelocity;
    private int collected;
    private bool won, muted;
    private GUIStyle titleStyle, smallStyle, numberStyle, bodyStyle;

    private void Start()
    {
        body = GetComponent<Rigidbody2D>();
        start = transform.position;
        cameraStart = sceneCamera.transform.position;
        lightStarts = new Vector3[lights.Length];
        for (int i = 0; i < lights.Length; i++) lightStarts[i] = lights[i].position;
        ambience = gameObject.AddComponent<AudioSource>();
        ambience.clip = wind;
        ambience.loop = true;
        ambience.volume = .32f;
        ambience.spatialBlend = 0;
        ambience.Play();
        effects = gameObject.AddComponent<AudioSource>();
        effects.volume = .5f;
        effects.spatialBlend = 0;
    }

    private void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) Restart();
            if (Keyboard.current.mKey.wasPressedThisFrame)
            {
                muted = !muted;
                ambience.mute = effects.mute = muted;
            }
        }
        maxHeight = Mathf.Max(maxHeight, transform.position.y - start.y);
        if (body.linearVelocity.y > 8 && previousVelocity < 3) effects.PlayOneShot(jump, .65f);
        previousVelocity = body.linearVelocity.y;
        bool grounded = Mathf.Abs(body.linearVelocity.y) < .08f;
        if (grounded && Mathf.Abs(body.linearVelocity.x) > .2f && Time.time > lastStep + .26f)
        {
            effects.pitch = Random.Range(.9f, 1.1f);
            effects.PlayOneShot(footstep, .27f);
            lastStep = Time.time;
        }
        if (Mathf.Abs(body.linearVelocity.x) > .1f)
            artwork.localScale = new Vector3(Mathf.Sign(body.linearVelocity.x), 1, 1);
        float bounce = grounded ? Mathf.Sin(Time.time * 18) * Mathf.Min(.028f, Mathf.Abs(body.linearVelocity.x) * .006f) : 0;
        artwork.localPosition = new Vector3(0, bounce, 0);
        scarf.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 6) * 8 + body.linearVelocity.y * 1.2f);
        for (int i = 0; i < lights.Length; i++)
        {
            if (!lights[i].gameObject.activeSelf) continue;
            lights[i].position = lightStarts[i] + Vector3.up * Mathf.Sin(Time.time * 2 + i) * .12f;
            if (Vector2.Distance(transform.position, lights[i].position) < .75f)
            {
                lights[i].gameObject.SetActive(false);
                collected++;
                effects.pitch = 1 + collected * .015f;
                effects.PlayOneShot(collect, .6f);
            }
        }
        if (!won && Vector2.Distance(transform.position, summit.position) < 1.15f)
        {
            won = true;
            effects.pitch = 1;
            effects.PlayOneShot(finish, .7f);
        }
        if (transform.position.y < sceneCamera.transform.position.y - 6.7f) Restart();
    }

    private void LateUpdate()
    {
        float target = Mathf.Max(cameraStart.y, transform.position.y + 1.5f);
        var p = sceneCamera.transform.position;
        p.y = Mathf.SmoothDamp(p.y, Mathf.Max(p.y, target), ref cameraVelocity, .35f);
        sceneCamera.transform.position = p;
        backdrop.position = new Vector3(0, p.y - cameraStart.y, 0);
        for (int i = 0; i < snowflakes.Length; i++)
        {
            var snow = snowflakes[i];
            var position = snow.localPosition;
            position.y -= Time.deltaTime * (.25f + (i % 5) * .16f);
            position.x += Time.deltaTime * (.12f + Mathf.Sin(Time.time * .6f + i) * .12f);
            if (position.y < -6.7f) position.y = 6.7f;
            if (position.x > 13) position.x = -13;
            snow.localPosition = position;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (effects == null) return;
        foreach (var contact in collision.contacts)
            if (contact.normal.y > .5f)
            {
                effects.pitch = 1;
                effects.PlayOneShot(landing, .35f);
                break;
            }
    }

    public void Restart()
    {
        GetComponent<PlayerMovement>().ResetMotion(start);
        sceneCamera.transform.position = cameraStart;
        cameraVelocity = maxHeight = previousVelocity = 0;
        collected = 0;
        won = false;
        foreach (var light in lights) light.gameObject.SetActive(true);
    }

    private void OnGUI()
    {
        if (titleStyle == null)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleStyle = new GUIStyle { font = font, fontSize = 28, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.91f,.98f,1) } };
            smallStyle = new GUIStyle(titleStyle) { fontSize = 12, fontStyle = FontStyle.Normal };
            smallStyle.normal.textColor = new Color(.63f,.8f,.86f);
            bodyStyle = new GUIStyle(titleStyle) { fontSize = 16, fontStyle = FontStyle.Normal };
            numberStyle = new GUIStyle(titleStyle) { fontSize = 30, alignment = TextAnchor.UpperRight };
        }
        float scale = Mathf.Min(Screen.width / 1440f, Screen.height / 900f);
        var oldMatrix = GUI.matrix;
        var oldColor = GUI.color;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
        float w = Screen.width / scale, h = Screen.height / scale;
        Panel(new Rect(28, 26, 282, 98), new Color(.025f,.085f,.14f,.83f));
        Panel(new Rect(28,26,3,98), new Color(.44f,.88f,.81f));
        GUI.Label(new Rect(49,40,250,20), "P O D R Ó Ż   P O D   Z O R Z Ą", smallStyle);
        GUI.Label(new Rect(47,65,270,40), "Zimowy szlak", titleStyle);
        Panel(new Rect(w-230,26,202,98), new Color(.025f,.085f,.14f,.83f));
        GUI.Label(new Rect(w-212,40,160,20), "WYSOKOŚĆ / ŚWIATEŁKA", smallStyle);
        GUI.Label(new Rect(w-208,63,160,42), $"{maxHeight * 10:0} m  ·  {collected}/{lights.Length}", numberStyle);
        Panel(new Rect(28,h-67,w-56,39), new Color(.025f,.085f,.14f,.8f));
        GUI.Label(new Rect(46,h-57,850,25), "A D / ← →   ruch      SPACJA   skok      R   od nowa      M   dźwięk " + (muted ? "wył." : "wł."), smallStyle);
        if (maxHeight < 2)
        {
            bodyStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(w/2-300,h-120,600,35), "Zbierz ciepłe światełka. Dotrzyj do latarni na szczycie.", bodyStyle);
            bodyStyle.alignment = TextAnchor.UpperLeft;
        }
        if (won)
        {
            Panel(new Rect(w/2-235,h/2-100,470,184), new Color(.025f,.085f,.14f,.95f));
            GUI.Label(new Rect(w/2-205,h/2-74,420,40), "Jesteś na szczycie!", titleStyle);
            GUI.Label(new Rect(w/2-205,h/2-25,420,35), $"Zebrane światełka: {collected} / {lights.Length}", bodyStyle);
            GUI.Label(new Rect(w/2-205,h/2+25,420,25), "Chwila pod zorzą. Naciśnij R, aby wyruszyć ponownie.", smallStyle);
        }
        GUI.matrix = oldMatrix;
        GUI.color = oldColor;
    }

    private static void Panel(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
