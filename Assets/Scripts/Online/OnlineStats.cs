using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Both are cumulated over an entire game

public struct ClientStats
{
    // Frames
    public int totalReceivedFrames;
    public int overwrittenServerFrames;
    public int totalFramesResimulated;
    public int totalFramesRewound;

    // Disagreements
    public int totalDifferenceChecks;
    public int totalSignificantDifferences;
    public int sumSignificantDifferenceSimulation;

    // History
    public int sumHistoryGamestates;
    public int sumHistoryInputs;
    public int countHistoryMeasure;

    // Speed up slow down multiplier
    public float sumBadSpeedMultipliers;
    public int countBadSpeedMultipliers;
    public int countSpeedMultipliers;

    // FPS
    public float sumFPS;
    public int countFPS;

    public override string ToString()
    {
        return
            "**Server received frames**" +
            "\nTotal: " + totalReceivedFrames +
            "\nOverwritten: " + Util.Percentage(overwrittenServerFrames / (float)totalReceivedFrames) +

            "\n\n**Client frames**" +
            "\nResimulated: " + totalFramesResimulated +
            "\nRewound: " + totalFramesRewound +

            "\n\n**Significant differences**" +
            "\nProportion of checks which failed: " + Util.Percentage(totalSignificantDifferences / (float)totalDifferenceChecks) +
            "\nAverage resimulation size: " + ((float)sumSignificantDifferenceSimulation / totalSignificantDifferences).ToString("0.00") +

            "\n\n**History**" +
            "\nAverage GameState count: " + ((float)sumHistoryGamestates / countHistoryMeasure).ToString("0.00") +
            "\nAverage InputPkg count: " + ((float)sumHistoryInputs / countHistoryMeasure).ToString("0.00") +

            "\n\n**Speed multiplier**" +
            "\nProportion which were bad: " + Util.Percentage(countBadSpeedMultipliers / (float)countSpeedMultipliers) +
            "\nAverage bad multiplier: " + (sumBadSpeedMultipliers / (0.1f + countBadSpeedMultipliers)).ToString("0.00") +

            "\n\n**FPS**" +
            "\nAverage FPS: " + (sumFPS / countFPS).ToString("0.000")

            + "\n";
    }
}

public struct ServerStats
{
    // Sending InputPkg
    public int totalInputSent;
    public int countInputSent;

    // Player queued inputs
    public int sumPlayerQueueSizes;
    public int countPlayerQueueSizes;

    // FPS
    public float sumFPS;
    public int countFPS;

    public override string ToString()
    {
        return
            "**Sent input packages**" +
            "\nTotal inputs sent: " + totalInputSent +
            "\nAverage length: " + (totalInputSent / (float)countInputSent).ToString("0.00") +

            "\n\n**FPS**" +
            "\nAverage FPS: " + (sumFPS / countFPS).ToString("0.000")

            + "\n";
    }
}
