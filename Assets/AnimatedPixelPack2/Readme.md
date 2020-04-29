# Animated Pixel Pack 2

This pack contains a set of over 20 (I plan to keep adding more over time) mini pixel characters that you can use for your own Unity games. It has fully commented source code, a set of 3 example games using the characters, and a host of other goodies. I hope you enjoy using, modifying, and generally having fun with it.

## Features
### Characters
* More than 20 character skin images made up of individual body parts
    * Located at /Characters
* Individual character prefabs ready to be dragged and dropped into a 2D platformer game
    * Located at /Characters/Prefabs
* A set of Unity Mecanim animations that are used by all of the characters
    * Located at /Characters/Animations
* A set of pre-generated sprite sheets for each character in case you don't want to use the Mecanim animations
    * Located at /Characters/Spritesheets
* A set of auto-generated outline sprites for each character that lets you customize a pixel border around each character
    * Located at /Characters/Outlines

### Editor
* A set of editor scripts that help with the creation of your own pixel characters in the same design
    * Located at /Editor

### Environment
* A bunch of additional sprites, tiles, prefabs, and materials used in the creation of the 3 demo scenes
    * Located at /Environment

### FXs and Projectiles
* A set of pre-made spell effects that can be mixed and matched with each character, and used as a basis to make your own
    * Located at /FX
* A set of pre-made projectiles that are also used by the characters
    * Located at /Projectiles

### Scenes
* A set of 3 different demo scenes that you can view to see how the characters work and give you some ideas of the sorts of games you could make.
    * /Scenes/2D_Platformer - A tile based platformer game example
    * /Scenes/2D_Match3 - A simple match 3 puzzle game example
    * /Scenes/2D_Survival - A simple top down survival type game example

### Scripts
* All fully commented C# script files used by the characters, demos, and prefabs
    * Located at /Scripts

### Spritesheet Generator
* A custom built scene and scripts used to generate the spritesheets (that aren't actually used in the demos but are provided just incase you want them). The scene will take a set of character prefabs and run each mecanim animation on them to record and generate spritesheets.


## Using the Demos
NOTE: The demos make use of the Unity pixel perfect camera script, which means you may need add that package via Window -> Package Manager -> 2D Pixel Perfect, if you get warnings about it being missing. You could also just delete script from each camera object, though the effect won't be as nice.

Each character has a specialized character controller script which allows you to drag and drop it into your own game and get started pretty quickly. They contain action controls to allow you to see all the animations. See the Character.cs and CharacterInput.cs source code for how it works.

Left Mouse - Quick attack
Right mouse - Attack
Middle mouse - Cast Special Ranged Attack (spell/gun/bow)
Z - Throw offhand item
X - Throw mainhand item
C - Consume offhand item
B - Block (if blocking is enabled on the character properties)
2 - Next character
1 - Previous character

### 2D Platformer
This is a simple example platformer, which is the main focus of the character prefabs.

Demo Controls:
Left/Right or A/D - character movement
Down or S - Crouch modifier
Space - Jump

### Match 3
This is a simple example puzzle game where you have to match 3 in a row. Doing so will cause your character to attack. Failing to match for a couple of turns will cause the enemy to attack.

Demo Controls:
Mouse click - select a block to swap to an adjacent position

### 2D Survival
This is a simple top down game where you can walk in all 4 directions and chop down trees by attacking them. Just to show you that you can use the characters in more than just traditional platformers.

Demo Controls:
Up/down/left/right or WSAD - movement
Left click - Attack tree


## Creating your own character skins and prefabs
I designed Animated Pixel Pack 2 with the idea that a lot of aspiring game developers don't have an artistic background but still want to be able to create cool looking pixel characters for their games. So instead of requiring many hours animating individual frames of animation for each new character to produce spritesheets, I created simplified pixel characters using individual body parts in a single image. These parts are split up and animated using Unity's built in mecanim animation tools in such a way that it looks like traditional spritesheet animation, but can be customized by the end user (you!).

### New Character Skins
* Create a new png image using your favorite paint program by using one from /Characters as an example
* Size 64x64
* In each 16x16 square, draw or customize the character body part
    * In order left to right, top to bottom it goes:
    * Head
    * Helm
    * Body
    * Pants
    * Head-Back (for ladder climbing)
    * Helm-Back (for ladder climbing)
    * Body-Back (for ladder climbing)
    * Elbow
    * Hands (used for both)
    * Feet (used for both)
    * Shoulder-Off (closest to camera)
    * Shoulder-Main (furthest from camera)
    * Main-Item
    * Off-Item
    * Attack FX (motion blur)
    * Head-Dead (for dying)
* It is probably best to start with an existing image so that all the parts line up in the correct place
* Leave at least 2 pixel space around each part in the 16x16 area
    * This is needed to avoid the generated outline from spilling out into another body part
* Save your new image into /Characters
* Unity will detect and import it
    * Due to the Editor scripts, it should get auto imported to use the correct settings (16 pixels per unit, multi sprite, 16x16 slices, point filtering, etc.)

### New Character Prefabs
Now to get your new skin onto a new character.
* Drag the base CHARACTER prefab from /Characters/Prefabs into the scene
* Duplicate this character prefab
* Rename this newly duplicated version
* Now drag your renamed prefab into /Characters/Prefabs and select "New Variant" so that it will always pick up changes from the base character
* Select your new character variant and look at the inspector
    * In the Character (Script) section you will see a Skin editor section which can be used to change all the body part sprites in one go
* Drag your previously created skin image from /Characters into the None (Texture 2D) box labeled 'Skin'
* Click the Apply Skin button
    * You should see the character prefab change into your new image
    * The script will also generate outlines from your skin and save them to /Characters/Outlines in a new subfolder
    * The script will also generate a  collider for the main item based on the shape of your body part in that section of the image
* Customize the rest of the character script, setting the weapon type, if they can block (holding a shield), the type of thrown and cast fx, etc.
* Apply the changes to your prefab variant using the Unity Overrides dropdown in the inspector

### Customizing Weapons/Spells/Effects
* The Weapon section of the character script shows the options available
* The Equipped Weapon Type select is used in the state machines to select which type of animation to play when you attack/cast (bow vs staff for example)
* IsBlockEnabled can be toggled to true if they are holding a shield to allow the block animation to play
* The LaunchPoint is an empty gameobject that specifies where projectiles should be launched from.
    * It can be moved from inside an animation
    * The animations use trigger points (see Humaonid_OverheadCast for an example) to specify when the projectile should fire
* CastProjectile should be a WeaponProjectile prefab that is cloned when the cast animation is triggered
* ThrowMainProjectile should be a WeaponProjectile prefab that will be cloned when the throw main item animation is triggered
* ThrowOffProjectile should be a WeaponProjectile prefab that will be cloned when the throw off item animation is triggered
* EffectPoint is an empty gameobject (similar to launch point) that specifies where an effect should be targetted during the cast animation
  See the Goblin_Tribalmancer character for an example lightning effect
* Effect is the prefab of the effect to cast

### Generating Spritesheets
If you have created some new Unity mecanim animations for the characters and want to automatically generate corresponding spritesheets for each prefab, you can use the Spritesheet_generator scene.
* Open the spritesheet_generator scene
   * Located at /SpritesheetGenerator
* Select the Spritesheetgenerator gameobject in the hierarchy
* In the inspector, Add the character prefabs that you would like to generate spritesheets for
* Click the 'Collect Frame Info' button
    * This will generate information about each animation in the character animation controller, and each keyframe in those animations
* Click run to start the scene
    * This will run through each character prefab from the list you added, and animate through each keyframe and generate the spritesheets under /Characters/Spritesheets


Hopefully most of the source code is self explanatory and you will be able to figure out how to use these features in your own game! If you have trouble, feel free to contact me at support@blanksourcecode.com

Thanks,

James
