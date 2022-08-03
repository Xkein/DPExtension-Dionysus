
-- lua script invoked by LuaComponent with global variables below
-- this:LuaComponent, Owner:IExtension

function OnFire(pTarget, weaponIndex)
    CS.DynamicPatcher.Logger.Log('master_mind.lua OnFire invoked!')
end

shareDamagePercent = 0.5

function OnReceiveDamage(pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse)
    local pCaptureManager = Owner.OwnerRef.CaptureManager

    local totalDamage = pDamage.Data

    local num = pCaptureManager.Ref.NumControlNodes
    -- print('Owner.OwnerObject:' .. tostring(Owner.OwnerObject))
    -- print('pCaptureManager:' .. tostring(pCaptureManager))
    for i = 0, num - 1 do
        local pNode = pCaptureManager.Ref.ControlNodes:Get(i)
        -- print('pNode:' .. tostring(pNode))
        
        pDamage.Data = totalDamage * shareDamagePercent / num
        local pUnit = pNode.Ref.Unit
        -- print('pUnit:' .. pUnit:ToString())
        -- print(pUnit.Ref.Base)
        pUnit.Ref.Base:ReceiveDamage(pUnit, pDamage, DistanceFromEpicenter, pWH, pAttacker, IgnoreDefenses, PreventPassengerEscape, pAttackingHouse)
        
    end
    if num > 0 then
        pDamage.Data = totalDamage * (1 - shareDamagePercent)
    end

end

