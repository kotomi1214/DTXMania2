# NativeDlls フォルダ

このフォルダには、DTXMania2 が必要とする**ネイティブDLL**を格納する。

C# プロジェクトでは、ビルド時または発行時にプロジェクトの依存するDLLが自動的に分析されて出力フォルダまたは発行フォルダに自動的にコピーされるが、ネイティブDLLの場合は依存関係を示すことができないため、これが行われない。
そのため、**ビルド時ならびに発行時に、出力フォルダにこれらを手動でコピーする必要がある**。

具体的には、DTXMania2 プロジェクトの「ポストビルドイベント」と「ポスト発行イベント」を使って、このフォルダ内のすべてのDLLを出力フォルダに xcopy する。
（なお、「ポスト発行イベント」は Visual Studio のプロジェクトプロパティから設定することはできないため、プロジェクトファイル(*.csproj)内に直接記述する。）

実際の記述：
```
  <Target Name="PostBuild" AfterTargets="PostBuildEvent">
    <Exec Command="xcopy /Y $(SolutionDir)packages-selfbuild\Effekseer-netcore\Ijwhost.dll $(TargetDir)&#xD;&#xA;xcopy /Y $(ProjectDir)NativeDlls\*.dll $(TargetDir)" />
  </Target>

  <!-- ポストビルドイベントでxcopyしたネイティブdllは publish 時には無視されるため、publish 後イベントで同じようにxcopyする。-->
  <Target Name="PostPublish" AfterTargets="Publish">
    <Exec Command="xcopy /Y $(SolutionDir)packages-selfbuild\Effekseer-netcore\Ijwhost.dll ..\publish&#xD;&#xA;xcopy /Y $(ProjectDir)NativeDlls\*.dll ..\publish&#xD;&#xA;" />
  </Target>
```
