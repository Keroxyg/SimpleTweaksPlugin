using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Interface.Utility;
using ImGuiNET;
using SimpleTweaksPlugin.Events;

namespace SimpleTweaksPlugin.Debugging;

public unsafe class EventsDebugging : DebugHelper {
    public override string Name => "Events";
    
    private Dictionary<AddonEvent, Dictionary<string, List<EventController.EventSubscriber>>>? addonEventDict;
    private List<EventController.EventSubscriber> frameworkSubscribers;
    
    private bool viewingFramework = false;
    private AddonEvent selectedType;
    
    public override void Draw() {
        addonEventDict ??= (Dictionary<AddonEvent, Dictionary<string, List<EventController.EventSubscriber>>>)typeof(EventController).GetProperty("AddonEventSubscribers", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);
        frameworkSubscribers ??= (List<EventController.EventSubscriber>)typeof(EventController).GetProperty("FrameworkUpdateSubscribers", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);

        if (addonEventDict == null || frameworkSubscribers == null) return;
        
        if (ImGui.BeginChild("eventTypeSelect", new Vector2(200 * ImGuiHelpers.GlobalScale, ImGui.GetContentRegionAvail().Y), true)) {
            if (ImGui.Selectable("Framework", viewingFramework)) {
                viewingFramework = true;
            }
            
            foreach (var t in Enum.GetValues<AddonEvent>()) {
                ImGui.BeginDisabled(addonEventDict == null || !addonEventDict!.ContainsKey(t));
                if (ImGui.Selectable($"{t}", selectedType == t && viewingFramework == false)) {
                    viewingFramework = false;
                    selectedType = t;
                }
                ImGui.EndDisabled();
            }
        }
        ImGui.EndChild();
        ImGui.SameLine();
        if (ImGui.BeginChild("events", ImGui.GetContentRegionAvail(), true)) {
            if (viewingFramework) {
                ShowEventList(frameworkSubscribers, true);
            } else {
                if (addonEventDict.TryGetValue(selectedType, out var selectedTypeDict)) {
                    var h = true;
                    foreach (var (addonName, list) in selectedTypeDict) {
                        
                        ImGui.SetWindowFontScale(1.25f);
                        ImGui.Text($"{addonName}");
                        ImGui.Separator();
                        ImGui.SetWindowFontScale(1f);

                        ShowEventList(list, h);
                        h = false;
                        
                        ImGui.Spacing();
                        ImGui.Spacing();
                    }
                }
            }
        }
        ImGui.EndChild();
        ImGui.SameLine();
    }

    private void ShowEventList(List<EventController.EventSubscriber> eventSubscribers, bool showHeader = false) {

        if (ImGui.BeginTable("eventTable", 3, ImGuiTableFlags.Borders)) {
            if (showHeader) {
                ImGui.TableSetupColumn("Tweak");
                ImGui.TableSetupColumn("Method");
                ImGui.TableSetupColumn("Invoke Type");
                ImGui.TableHeadersRow();
            }
            
            foreach (var s in eventSubscribers) {

                ImGui.TableNextColumn();
                ImGui.Text($"{s.Tweak.Name}");
                ImGui.TableNextColumn();
                ImGui.Text($"{s.Method.Name}");
                ImGui.TableNextColumn();
                ImGui.Text($"{s.Kind}");
            }
            
            ImGui.EndTable();
        }
    }
}

