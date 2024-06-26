module CelesteTwitchIntegrationSampleTrigger

using ..Ahorn, Maple

@mapdef Trigger "CelesteTwitchIntegration/SampleTrigger" SampleTrigger(
    x::Integer, y::Integer, width::Integer=Maple.defaultTriggerWidth, height::Integer=Maple.defaultTriggerHeight,
    sampleProperty::Integer=0
)

const placements = Ahorn.PlacementDict(
    "Sample Trigger (CelesteTwitchIntegration)" => Ahorn.EntityPlacement(
        SampleTrigger,
        "rectangle",
    )
)

end