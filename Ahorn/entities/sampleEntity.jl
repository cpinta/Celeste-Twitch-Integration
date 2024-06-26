module CelesteTwitchIntegrationSampleEntity

using ..Ahorn, Maple

@mapdef Entity "CelesteTwitchIntegration/SampleEntity" SampleEntity(
    x::Integer, y::Integer,
    sampleProperty::Integer=0
)

const placements = Ahorn.PlacementDict(
    "Sample Entity (CelesteTwitchIntegration)" => Ahorn.EntityPlacement(
        SampleEntity,
    )
)

sprite = "objects/CelesteTwitchIntegration/sampleEntity/idle00"

function Ahorn.selection(entity::SampleEntity)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SampleEntity, room::Maple.Room) = Ahorn.drawSprite(ct, sprite, 0, 0)

end