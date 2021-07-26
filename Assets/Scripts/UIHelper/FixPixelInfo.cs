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
    Communication comm;
    void Start()
    {
        comm = GameObject.Find("Manager").GetComponent<Communication>();
        StartCoroutine(UpdateFixPixelInfo());
    }

    IEnumerator UpdateFixPixelInfo()
    {
        xy = this.gameObject.GetComponent<InputField>().text = $"{comm.PixelWidth / 2},{comm.PixelHeight / 2}";
        while (true)
        {
            xy = this.gameObject.GetComponent<InputField>().text;
            var xys = xy.Split(new char[] { ' ', ',', '，' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (xys.Length == 2)
            {
                int x, y;
                if (int.TryParse(xys[0], out x))
                {
                    if (x > comm.PixelWidth - 1 || x < 0)
                    {
                        Info.text = $"OutOfRange:{x}";
                    }
                    else
                    {
                        if (int.TryParse(xys[1], out y))
                        {
                            if (y > comm.PixelHeight - 1 || y < 0)
                            {
                                Info.text = $"OutOfRange:{y}";
                            }
                            else
                            {
                                var pixelSN = new Vector2Int(x, y);
                                Info.text = $"=> {DepthHelper.GetBufferData(pixelSN)}mm {GrayHelper.GetBufferData(pixelSN)}LSB";
                            }
                        }
                        else
                        {
                            Info.text = $"formal error:{xys[1]}";
                        }
                    }
                }
                else
                {
                    Info.text = $"formal error:{xys[0]}";
                }
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
