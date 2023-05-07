using System.Collections.Generic;
using System;

namespace MyGame.Utility;

public class ModifierItem<T> {
    public string Name {get; init;}
    public T Modification {get; init;}
    public bool Disabled {get; set;}
    public List<string> overridenBy = new();

    public ModifierItem( string name, T multiplier ) {
        this.Name = name;
        this.Modification = multiplier;
    }

    public ModifierItem(string name, T multiplier, params string[] overridenBy) {
        this.Name = name;
        this.Modification = multiplier;
        this.overridenBy = new List<string>(overridenBy);
    }
}

public class Modifier<T> {
    private readonly List<ModifierItem<T>> Modifiers = new();

    public void Disable( string name ) {
        var modifier = Modifiers.Find( m => m.Name == name );

        if(modifier != null) {
            modifier.Disabled = true;
        }
    }

    public void Enable( string name ) {
        var modifier = Modifiers.Find( m => m.Name == name );

        if(modifier != null) {
            modifier.Disabled = false;
        }
    }

    public void Remove( string name ) {
        var modifier = Modifiers.Find( m => m.Name == name );

        if(modifier != null) {
            Modifiers.Remove( modifier );
        }
    }

    public void Add( ModifierItem<T> modifier ) {
        if(Modifiers.Find(m => modifier.Name == m.Name) != null) {
            return;
        }
        Modifiers.Add( modifier );
        Log.Info( $"Added {modifier.Name}" );
    }

    public void ForEach(Action<ModifierItem<T>> action) {
        Modifiers.ForEach(a => {
            if(a.Disabled) return;
            
            if(Modifiers.Find(m => !a.Disabled && a.overridenBy.Contains(m.Name)) != null) {
                return;
            }

            action(a);
        });
    }
}