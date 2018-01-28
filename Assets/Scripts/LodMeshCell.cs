﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LodMeshCell
{
    public int CurrentLod { get; set; }


    private int m_MinLod;
    private int m_MaxLod;
    private int m_CellX;
    private int m_CellY;
    private float m_Scale;

    public LodMeshCell(int minLod, int maxlod, int cellX, int cellY, float scale)
    {
        this.m_MinLod = minLod;
        this.m_MaxLod = maxlod;
        this.CurrentLod = minLod;
        this.m_CellX = cellX;
        this.m_CellY = cellY;
        this.m_Scale = scale;
    }

    public void UpdateMesh(List<Vector3> vlist, List<int> ilist, int leftLod, int rightLod, int upLod, int downLod,
        ref int index)
    {
        int xw = (int) Mathf.Pow(2, CurrentLod);
        int yw = xw;
        UpdateMesh_InternalLod(vlist, ilist, xw, yw, leftLod, rightLod, upLod, downLod, ref index);
    }

    private void UpdateMesh_InternalLod(List<Vector3> vlist, List<int> ilist, int xwidth, int ywidth, int leftLod,
        int rightLod, int upLod, int downLod, ref int index)
    {
        int firstIndex = index;
        if (CurrentLod == 0)
        {
            vlist.Add(new Vector3(m_CellX*m_Scale, 0, m_CellY*m_Scale));
            vlist.Add(new Vector3(m_CellX*m_Scale + m_Scale, 0, m_CellY*m_Scale));
            vlist.Add(new Vector3(m_CellX*m_Scale, 0, m_CellY*m_Scale + m_Scale));
            vlist.Add(new Vector3(m_CellX*m_Scale + m_Scale, 0, m_CellY*m_Scale + m_Scale));

            ilist.Add(index + 0);
            ilist.Add(index + 2);
            ilist.Add(index + 1);

            ilist.Add(index + 1);
            ilist.Add(index + 2);
            ilist.Add(index + 3);

            index += 4;
            return;
        }
        else if (CurrentLod == 1)
        {
            vlist.Add(new Vector3(m_CellX*m_Scale + 0.5f*m_Scale, 0, m_CellY*m_Scale + 0.5f*m_Scale));
            index += 1;
        }
        else
        {
            for (int i = 1; i < ywidth; i++)
            {
                for (int j = 1; j < xwidth; j++)
                {
                    float x = ((float) j)/xwidth*m_Scale + m_CellX*m_Scale;
                    float z = ((float) i)/ywidth*m_Scale + m_CellY*m_Scale;
                    Vector3 pos = new Vector3(x, 0, z);
                    vlist.Add(pos);
                    if (j != xwidth - 1 && i != ywidth - 1)
                    {
                        ilist.Add(index + (i - 1)*(xwidth - 1) + j - 1);
                        ilist.Add(index + (i)*(xwidth - 1) + j - 1);
                        ilist.Add(index + (i - 1)*(xwidth - 1) + j);

                        ilist.Add(index + (i - 1)*(xwidth - 1) + j);
                        ilist.Add(index + (i)*(xwidth - 1) + j - 1);
                        ilist.Add(index + (i)*(xwidth - 1) + j);
                    }
                }
            }
            index += (ywidth - 1)*(xwidth - 1);
        }

        vlist.Add(new Vector3(m_CellX*m_Scale, 0, m_CellY*m_Scale));
        vlist.Add(new Vector3(m_CellX*m_Scale + m_Scale, 0, m_CellY*m_Scale));
        vlist.Add(new Vector3(m_CellX*m_Scale, 0, m_CellY*m_Scale + m_Scale));
        vlist.Add(new Vector3(m_CellX*m_Scale + m_Scale, 0, m_CellY*m_Scale + m_Scale));

        int lbindex = index;
        int rbindex = index + 1;
        int luindex = index + 2;
        int ruindex = index + 3;

        index += 4;
        
        UpdateMeshHorizontalEdge(vlist, ilist, m_CellY * m_Scale, xwidth, downLod, lbindex, rbindex, firstIndex, true, ref index);
        UpdateMeshHorizontalEdge(vlist, ilist, m_CellY * m_Scale + m_Scale, xwidth, upLod, luindex, ruindex, firstIndex + (xwidth - 1) * (ywidth - 2), false, ref index);
        UpdateMeshVerticalEdge(vlist, ilist, m_CellX * m_Scale, xwidth, ywidth, leftLod, lbindex, luindex, firstIndex, true, ref index);
        UpdateMeshVerticalEdge(vlist, ilist, m_CellX * m_Scale + m_Scale, xwidth, ywidth, rightLod, rbindex, ruindex, firstIndex +xwidth - 2, false, ref index);
    }

    private void UpdateMeshHorizontalEdge(List<Vector3> vlist, List<int> ilist, float z, int edgeWidth, int neighborLod,
        int leftIndex, int rightIndex, int firstIndex, bool clockWise, ref int index)
    {
        int deltaLod = Mathf.Max(0, CurrentLod - neighborLod);
        int step = (int) Mathf.Pow(2, deltaLod);
        int sp = deltaLod*(deltaLod - 1);
        int offset = deltaLod == 0 ? 0 : (int) Mathf.Pow(2, deltaLod - 1) - 1;
        for (int i = 0; i <= edgeWidth; i += step)
        {
            int ind = i/step;
            if (i != 0 && i != edgeWidth)
            {
                float x = ((float) i)/edgeWidth*m_Scale + m_CellX*m_Scale;
                vlist.Add(new Vector3(x, 0, z));
            }
            if (i != edgeWidth)
            {
                if (i == 0)
                    ilist.Add(leftIndex);
                else
                    ilist.Add(index + ind - 1);
                if (clockWise)
                {
                    if (i == edgeWidth - 1)
                        ilist.Add(firstIndex + edgeWidth - 2);
                    else
                        ilist.Add(firstIndex + i + offset);
                    if (i == edgeWidth - step)
                        ilist.Add(rightIndex);
                    else
                        ilist.Add(index + ind + 1 - 1);
                }
                else
                {
                    if (i == edgeWidth - step)
                        ilist.Add(rightIndex);
                    else
                        ilist.Add(index + ind + 1 - 1);
                    if (i == edgeWidth - 1)
                        ilist.Add(firstIndex + edgeWidth - 2);
                    else
                        ilist.Add(firstIndex + i + offset);
                }
            }
            if (i > 0 && i <= edgeWidth - step)
            {
                if (deltaLod != 0 || i != edgeWidth - 1)
                {
                    ilist.Add(index + ind - 1);
                    if (clockWise)
                    {
                        ilist.Add(firstIndex + i - 1);
                        ilist.Add(firstIndex + i);
                    }
                    else
                    {
                        ilist.Add(firstIndex + i);
                        ilist.Add(firstIndex + i - 1);
                    }
                }
            }
            if (deltaLod != 0)
            {
                if (i >= 0 && i < edgeWidth - step)
                {
                    if (clockWise)
                    {
                        ilist.Add(firstIndex + i + sp);
                        ilist.Add(firstIndex + i + sp + 1);
                    }
                    else
                    {
                        ilist.Add(firstIndex + i + sp + 1);
                        ilist.Add(firstIndex + i + sp);
                    }
                    ilist.Add(index + ind + 1 - 1);
                }

                if (i >= 0 && i <= edgeWidth - step)
                {
                    int bindex = i == 0 ? leftIndex : (index + ind - 1);
                    int eindex = i == edgeWidth - step ? rightIndex : (index + ind);
                    for (int j = 0; j < step - 2; j++)
                    {
                        if (j < offset)
                            ilist.Add(bindex);
                        else
                            ilist.Add(eindex);
                        if (clockWise)
                        {
                            ilist.Add(firstIndex + i + j);
                            ilist.Add(firstIndex + i + j + 1);
                        }
                        else
                        {
                            ilist.Add(firstIndex + i + j + 1);
                            ilist.Add(firstIndex + i + j);
                        }

                    }

                }
            }
        }
        index += deltaLod == 0 ? (edgeWidth - 1) : (edgeWidth - 2)/step;
    }

    private void UpdateMeshVerticalEdge(List<Vector3> vlist, List<int> ilist, float x, int xwidth, int ywidth, int neighborLod,
        int bottomIndex, int upIndex, int firstIndex, bool clockWise, ref int index)
    {
        int deltaLod = Mathf.Max(0, CurrentLod - neighborLod);
        int step = (int) Mathf.Pow(2, deltaLod);
        int sp = deltaLod*(deltaLod - 1);
        int offset = deltaLod == 0 ? 0 : (int) Mathf.Pow(2, deltaLod - 1) - 1;
        for (int i = 0; i <= ywidth; i += step)
        {
            int ind = i/step;
            if (i != 0 && i != ywidth)
            {
                float z = ((float) i)/ywidth*m_Scale + m_CellY*m_Scale;
                vlist.Add(new Vector3(x, 0, z));
            }
            if (i != ywidth)
            {
                if (i == 0)
                    ilist.Add(bottomIndex);
                else
                    ilist.Add(index + ind - 1);
                if (clockWise)
                {
                    if (i == ywidth - step)
                        ilist.Add(upIndex);
                    else
                        ilist.Add(index + ind + 1 - 1);
                    if (i == ywidth - 1)
                        ilist.Add(firstIndex + (ywidth - 2) * (xwidth - 1));
                    else
                        ilist.Add(firstIndex + (i + offset) * (xwidth - 1));
                }
                else
                {
                    if (i == ywidth - 1)
                        ilist.Add(firstIndex + (ywidth - 2)*(xwidth - 1));
                    else
                        ilist.Add(firstIndex + (i + offset)*(xwidth - 1));

                    if (i == ywidth - step)
                        ilist.Add(upIndex);
                    else
                        ilist.Add(index + ind + 1 - 1);
                }
            }
            if (i > 0 && i <= ywidth - step)
            {
                if (deltaLod != 0 || i != ywidth - 1)
                {
                    ilist.Add(index + ind - 1);
                    if (clockWise)
                    {
                        ilist.Add(firstIndex + (i)*(xwidth - 1));
                        ilist.Add(firstIndex + (i - 1)*(xwidth - 1));
                    }
                    else
                    {
                        ilist.Add(firstIndex + (i - 1)*(xwidth - 1));
                        ilist.Add(firstIndex + (i)*(xwidth - 1));
                    }
                }
            }
            if (deltaLod != 0)
            {
                if (i >= 0 && i < ywidth - step)
                {
                    if (clockWise)
                    {
                        ilist.Add(firstIndex + (i + sp + 1)*(xwidth - 1));
                        ilist.Add(firstIndex + (i + sp)*(xwidth - 1));
                    }
                    else
                    {
                        ilist.Add(firstIndex + (i + sp)*(xwidth - 1));
                        ilist.Add(firstIndex + (i + sp + 1)*(xwidth - 1));
                    }
                    ilist.Add(index + ind + 1 - 1);
                }

                if (i >= 0 && i <= ywidth - step)
                {
                    int bindex = i == 0 ? bottomIndex : (index + ind - 1);
                    int eindex = i == ywidth - step ? upIndex : (index + ind);
                    for (int j = 0; j < step - 2; j++)
                    {
                        if (j < offset)
                            ilist.Add(bindex);
                        else
                            ilist.Add(eindex);
                        if (clockWise)
                        {
                            ilist.Add(firstIndex + (i + j + 1) * (xwidth - 1));
                            ilist.Add(firstIndex + (i + j) * (xwidth - 1));
                        }
                        else
                        {
                            ilist.Add(firstIndex + (i + j)*(xwidth - 1));
                            ilist.Add(firstIndex + (i + j + 1)*(xwidth - 1));
                        }
                    }

                }
            }
        }
        index += deltaLod == 0 ? (ywidth - 1) : (ywidth - 2)/step;
    }
}
