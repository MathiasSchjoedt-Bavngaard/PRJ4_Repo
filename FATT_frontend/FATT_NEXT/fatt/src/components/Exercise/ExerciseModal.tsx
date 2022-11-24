import NumberField from "../NumberField";
import { useState } from "react";
import { useUpdateWeight } from "../../mutation/account/PutWeight";
import InputButton from "../Button/InputButton";
import InputField from "../InputField";
import { usePostExercise } from "../../mutation/exercise/PostExercise";
import { ExerciseNoIdDto } from "../../../interfaces/Exercise";

export default function ExerciseModal() {
  const { mutate: exercise } = usePostExercise();

  const [exerciseName, setExerciseName] = useState("");
  const [category, setCategory] = useState("");
  const [Equipment, setEquipment] = useState("");
  const [Intensity, setIntensity] = useState("");
  const [Repetitions, setRepetitions] = useState("");
  const [Sets, setSets] = useState("");
  const [PicturePath, setPicturePath] = useState("");
  const [VideoPath, setVideoPath] = useState("");

  const handleCreateButtonClick = (e) => {
    e.preventDefault();
    const exerciseNoIdDto: ExerciseNoIdDto = {
      Name: exerciseName,
      Category: category,
      Intensity: Intensity,
      Equipment: Equipment,
      Repetitions: Repetitions,
      Sets: Sets,
      PicturePath: PicturePath,
      VideoPath: VideoPath,
    };
    exercise(exerciseNoIdDto);
  };

  return (
    <div className="p-6">
      <h3 className="text-xl font-semibold text-gray-900 mb-5">
        Create exercise
      </h3>
      <form
        onSubmit={handleCreateButtonClick}
        className="flex flex-wrap border rounded bg-grey-200"
      >
        <InputField
          type="text"
          placeholder="Name"
          onChange={(e) => setExerciseName(e.target.value)}
          value={undefined}
          required
        />
        <InputField
          type="text"
          placeholder="Category"
          onChange={(e) => setCategory(e.target.value)}
          value={undefined}
          required
        />
        <InputField
          type="text"
          placeholder="Intensity"
          onChange={(e) => setIntensity(e.target.value)}
          value={undefined}
          required
        />
        <InputField
          type="text"
          placeholder="Equipment"
          onChange={(e) => setEquipment(e.target.value)}
          value={undefined}
          required
        />
        <InputField
          type="text"
          placeholder="Repetitions"
          onChange={(e) => setRepetitions(e.target.value)}
          value={undefined}
          required
        />
        <InputField
          type="text"
          placeholder="Sets"
          onChange={(e) => setSets(e.target.value)}
          value={undefined}
          required
        />
        <InputField
          type="text"
          placeholder="PicturePath"
          onChange={(e) => setPicturePath(e.target.value)}
          value={undefined}
          required
        />
        <InputField
          type="text"
          placeholder="VideoPath"
          onChange={(e) => setVideoPath(e.target.value)}
          value={undefined}
          required
        />
      </form>
      <InputButton
        type={"onSubmit"}
        onClick={handleCreateButtonClick}
        text={"Create"}
        key={undefined}
      />
    </div>
  );
}

// <div>
// <h1 className="mb-2 font-bold">Create an exercise</h1>
