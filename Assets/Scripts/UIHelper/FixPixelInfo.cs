using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixPixelInfo : MonoBehaviour
{

    public GrayShaderHelper GrayHelper;
    public DepthShaderHelper DepthHelper;
    // Use this for initialization
    public Text Info;
    private string xy;
    void Start()
    {
        xy = this.gameObject.GetComponent<InputField>().text = "320,240";
        StartCoroutine(UpdateFixPixelInfo());
    }

    IEnumerator UpdateFixPixelInfo()
    {
        while (true)
        {
            var xys = xy.Split(new char[] { ' ', ',', '，' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (xys.Length == 2)
            {
                var pixelSN = new Vector2Int(int.Parse(xys[0]), int.Parse(xys[1]));
                Info.text = $"=> {DepthHelper.GetBufferData(pixelSN)}mm {GrayHelper.GetBufferData(pixelSN)}LSB";
            }
            else
            {
                Info.text = $"formal error:{xy}";
            }
            yield return new WaitForSeconds(1);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
